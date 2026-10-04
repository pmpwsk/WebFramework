class LoadingScreen {
    static show() {
        console.log("Loading screen activated.");
        let loadingScreen = getElementByPath(["body", "loading"]);
        if (loadingScreen)
            addClass(loadingScreen, "wf-is-open");
    }

    static hide() {
        console.log("Loading screen deactivated.");
        let loadingScreen = getElementByPath(["body", "loading"]);
        if (loadingScreen)
            removeClass(loadingScreen, "wf-is-open");
    }
}

class WrappedSocket {
    onReconnectingAsync = null;
    onConnectedAsync = null;
    onMessageAsync = null;
    
    #url;
    #reconnectEnabled = true;
    #socket = null;
    
    constructor(url) {
        this.#url = url;
    }
    
    startAsync = async () => {
        this.#reconnectEnabled = true;
        try {
            this.#socket = new WebSocket(this.#url);
            this.unloadHandler = this.stopAsync;
            window.addEventListener("beforeunload", this.unloadHandler);
            this.#socket.addEventListener("open", this.#onOpenAsync);
            this.#socket.addEventListener("close", this.#onCloseAsync);
            this.#socket.addEventListener("message", this.#onMessageAsync);
        } catch (error) {
            console.log("Socket creation failed.");
            throw error;
        }
    }
    
    stopAsync = async () => {
        this.#reconnectEnabled = false;
        window.removeEventListener("beforeunload", this.stopAsync);
        this.#socket?.close();
        this.#socket = null;
    }
    
    restartAsync = async () => {
        await this.stopAsync();
        await new Promise(resolve => {
            setTimeout(async () => {
                await this.startAsync();
                resolve();
            }, 0)
        });
    }
    
    #onOpenAsync = async () => {
        console.log("Socket event 'open'.");
        await this.onConnectedAsync?.();
    }
    
    #onCloseAsync = async () => {
        console.log("Socket event 'close'.");
        this.#socket = null;
        if (this.#reconnectEnabled) {
            console.log("Socket reconnecting.")
            await this.onReconnectingAsync?.();
            setTimeout(this.startAsync, 2000);
        }
    }
    
    #onMessageAsync = async (event) => {
        console.log("Socket event 'message'.");
        await this.onMessageAsync?.(event.data);
    }
}

let watcherId = null;
let watcher = null;

if (document.documentElement.hasAttribute("data-wf-url")) {
    let url = document.documentElement.getAttribute("data-wf-url");
    watcher = new WrappedSocket(`/wf/dyn/watcher?url=${encodeURIComponent(url)}`);
    watcher.onReconnectingAsync = LoadingScreen.show;
    watcher.onMessageAsync = onWatcherMessageAsync;
    (async () => watcher.startAsync())();
}

document.addEventListener("click", event =>
{
    let target = getClickReceiver(event.target)
    if (target.matches(".wf-nav-menu-toggle"))
    {
        // Toggle nav menu
        closeAllPopups();
        toggleClass(findAside(), "wf-is-forced");
    }
    else if (target.matches(".wf-popup-toggle"))
    {
        // Toggle other menu
        let popup = resolveTarget(target);
        if (popup && popup.matches(".wf-menu, .wf-dialog"))
            openPopup(popup);
    }
    else if (target.matches(".wf-menu-background, aside .wf-button, .wf-menu .wf-button"))
    {
        // Close all popups
        removeClass(findAside(), "wf-is-forced");
        closeAllPopups();
    }
    else if (target.matches(".wf-image"))
    {
        // Toggle image fullscreen
        if (target.matches(".wf-fullscreen"))
        {
            // Remove fullscreen image
            target.remove();
        }
        else
        {
            // Add fullscreen image
            let fullscreenImage = target.cloneNode();
            fullscreenImage.classList.add("wf-fullscreen");
            document.body.append(fullscreenImage);
        }
    }
});

document.addEventListener("submit", event =>
{
    if (watcherId && event.submitter && event.submitter.matches(".wf-server-form-override"))
    {
        // Form with overriden server action
        event.preventDefault();
        runServerAction(event.submitter, event.target);
    }
    else if (watcherId && event.target.matches(".wf-server-form"))
    {
        // Form with server action
        event.preventDefault();
        runServerAction(event.target, event.target);
    }
});

document.addEventListener("keydown", event =>
{
    let value = getValueForForm(event.target);
    if (value !== undefined)
        event.target.setAttribute("data-wf-modified", "");
});

document.addEventListener("change", event =>
{
    let value = getValueForForm(event.target);
    if (value !== undefined)
        event.target.setAttribute("data-wf-modified", "");
});

async function onWatcherMessageAsync(data) {
    let change = JSON.parse(data);
    switch (change.type) {
        case "Navigate":
            window.location.assign(change.location);
            break;
        case "Welcome":
            watcherId = change.id;
            break;
        case "FullPage":
            let script = getElementByPath(["body", "script"]);
            if (script && script.getAttribute("src") !== change.script) {
                window.location.reload();
                break;
            }

            let valueMap = new Map();
            writeAllValuesToMap(document.body, valueMap);
            let focusName = document.activeElement?.name;

            document.head.innerHTML = "";
            for (let html of change.head)
                document.head.append(parseElement(html));

            for (let child of [...document.body.children])
                if (!matchesSystemId(child, "script") && !matchesSystemId(child, "loading"))
                    child.remove();

            for (let html of change.beforeScript.reverse())
                document.body.prepend(parseElement(html));

            for (let html of change.afterScript)
                document.body.append(parseElement(html));

            writeAllValuesFromMap(document.body, valueMap);

            LoadingScreen.hide();

            if (focusName)
                document.getElementsByName(focusName)[0].focus();
            break;
        case "AttributeChanged": {
            let element = getElementByPath(change.path);
            if (element)
                if (change.attributeValue)
                    element.setAttribute(change.attributeName, change.attributeValue);
                else element.removeAttribute(change.attributeName);
        }
            break;
        case "ElementRemoved": {
            let element = getElementByPath(change.path);
            if (element)
                element.remove();
        }
            break;
        case "ElementAddedBefore": {
            let successor = getElementByPath(change.path);
            if (successor) {
                let element = parseElement(change.html);
                successor.parentNode.insertBefore(element, successor);
            }
        }
            break;
        case "ElementAddedAfter": {
            let predecessor = getElementByPath(change.path);
            if (predecessor) {
                let element = parseElement(change.html);
                let successor = predecessor.nextSibling;
                if (successor)
                    successor.parentNode.insertBefore(element, successor);
                else
                    predecessor.parentNode.append(element);
            }
        }
            break;
        case "ContentChanged": {
            let element = getElementByPath(change.path);
            if (element)
                element.innerHTML = change.content;
        }
            break;
        case "SetValue": {
            let element = getElementByPath(change.path);
            if (element)
                element.value = change.value;
        }
            break;
        case "InternalReload": {
            LoadingScreen.show();
            await watcher.restartAsync();
        }
            break;
        default: {
            console.warn("Unknown change", change);
        }
            break;
    }
}

function getClickReceiver(target)
{
    let element = target;
    while (element)
    {
        if (element.matches(".wf-button"))
            return element;
        element = element.parentElement;
    }

    return target;
}

function findAside()
{
    return document.querySelector("aside");
}

function openPopup(popup)
{
    removeClass(findAside(), "wf-is-forced");
    closeAllPopups(popup);
    toggleClass(popup, "wf-is-open");
}

function closeAllPopups(except)
{
    for (let popup of document.querySelectorAll(".wf-menu, .wf-dialog"))
        if (!except || popup !== except)
            removeClass(popup, "wf-is-open");
}

function toggleClass(target, name)
{
    if (target.classList.contains(name))
        target.classList.remove(name);
    else
        target.classList.add(name);
}

function removeClass(target, name)
{
    if (target.classList.contains(name))
        target.classList.remove(name);
}

function addClass(target, name)
{
    if (!target.classList.contains(name))
        target.classList.add(name);
}

function resolveTarget(element)
{
    return element.hasAttribute("data-wf-target-id")
        ? document.getElementById(element.getAttribute("data-wf-target-id"))
        : null;
}

function getElementByPath(path)
{
    let node = document.documentElement;
    for (let id of path)
    {
        node = getElementBySystemId(node, id);
        if (!node)
            return null;
    }

    return node;
}

function matchesSystemId(element, id)
{
    return element.hasAttribute("data-wf-id") && element.getAttribute("data-wf-id") === id;
}

function getElementBySystemId(parent, id)
{
    for (let child of parent.children)
        if (matchesSystemId(child, id))
            return child;
    return null;
}

function getSystemPath(element)
{
    let path = [];
    while (element && element.hasAttribute("data-wf-id"))
    {
        path.push(element.getAttribute("data-wf-id"));
        element = element.parentElement;
    }
    return path.reverse();
}

function runServerAction(submitter, form)
{
    LoadingScreen.show();
    
    let request = new XMLHttpRequest();
    request.open("POST", `/wf/dyn/submit?id=${watcherId}&path=${encodeURIComponent(JSON.stringify(getSystemPath(submitter)))}`);
    request.onload = () =>
    {
        let action = JSON.parse(request.responseText);
        let stopLoading = true;
        switch (action.type)
        {
            case "Nothing":
                break;
            case "Navigate":
                stopLoading = false;
                window.location.assign(action.location);
                break;
            case "Reload":
                stopLoading = false;
                window.location.reload();
                break;
            default:
                console.warn("Unknown action", action);
                break;
        }

        if (stopLoading)
            LoadingScreen.hide();
    }
    let formData = new FormData();
    appendAllToForm(form, formData);
    request.send(formData);
}

function getValueForForm(element)
{
    if (element.matches(".wf-textbox"))
        return element.value;
    return undefined;
}

function appendAllToForm(element, formData)
{
    let value = getValueForForm(element);
    if (value !== undefined)
        formData.append(JSON.stringify(getSystemPath(element)), value);
    
    for (let child of element.children)
        appendAllToForm(child, formData);
}

function writeAllValuesToMap(element, map)
{
    let valueWriter = getValueWriter(element);
    if (element.name && valueWriter)
        map.set(element.name, valueWriter);

    for (let child of element.children)
        writeAllValuesToMap(child, map);
}

function writeAllValuesFromMap(element, map)
{
    if (element.name)
    {
        let valueWriter = map.get(element.name);
        if (valueWriter)
            valueWriter(element);
    }

    for (let child of element.children)
        writeAllValuesFromMap(child, map);
}

function getValueWriter(element)
{
    if (!element.name || !element.hasAttribute("data-wf-modified"))
        return null;

    if (element.matches(".wf-textbox"))
    {
        let value = element.value;
        return otherElement =>
        {
            if (otherElement.matches(".wf-textbox"))
            {
                otherElement.value = value;
                otherElement.setAttribute("data-wf-modified", "");
            }
        };
    }
}

function parseElement(html)
{
    let template = document.createElement("template");
    template.innerHTML = html;
    return template.content.firstChild;
}