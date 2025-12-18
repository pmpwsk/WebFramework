let watcherId = null;

if (document.documentElement.hasAttribute("data-wf-url"))
{
    let url = document.documentElement.getAttribute("data-wf-url");
    let watcher = new EventSource(`/wf/dyn/watcher?url=${encodeURIComponent(url)}`);
    onbeforeunload = () => watcher.close();
    watcher.onmessage = async event =>
    {
        if (event.data.startsWith(":"))
            return;
        
        let change = JSON.parse(event.data);
        switch (change.type)
        {
            case "Navigate":
                window.location.assign(change.location);
                break;
            case "Welcome":
                watcherId = change.id;
                break;
            case "FullPage":
                let script = getElementByPath(["body", "script"]);
                if (script && script.getAttribute("src") !== change.script)
                {
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
                    if (!matchesSystemId(child, "script"))
                        child.remove();
                
                for (let html of change.beforeScript.reverse())
                    document.body.prepend(parseElement(html));

                for (let html of change.afterScript)
                    document.body.append(parseElement(html));
                
                writeAllValuesFromMap(document.body, valueMap);
                if (focusName)
                    document.getElementsByName(focusName)[0].focus();
                break;
            case "AttributeChanged":
            {
                let element = getElementByPath(change.path);
                if (element)
                    if (change.attributeValue)
                        element.setAttribute(change.attributeName, change.attributeValue);
                    else element.removeAttribute(change.attributeName);
            } break;
            case "ElementRemoved":
            {
                let element = getElementByPath(change.path);
                if (element)
                    element.remove();
            } break;
            case "ElementAddedBefore":
            {
                let successor = getElementByPath(change.path);
                if (successor)
                {
                    let element = parseElement(change.html);
                    successor.parentNode.insertBefore(element, successor);
                }
            } break;
            case "ElementAddedAfter":
            {
                let predecessor = getElementByPath(change.path);
                if (predecessor)
                {
                    let element = parseElement(change.html);
                    let successor = predecessor.nextSibling;
                    if (successor)
                        successor.parentNode.insertBefore(element, successor);
                    else
                        predecessor.parentNode.append(element);
                }
            } break;
            case "ContentChanged":
            {
                let element = getElementByPath(change.path);
                if (element)
                    element.innerHTML = change.content;
            } break;
            default:
            {
                console.warn("Unknown change", change);
            } break;
        }
    }
}

document.addEventListener("click", event =>
{
    if (event.target.matches(".wf-nav-menu-toggle"))
    {
        // Toggle nav menu
        closeAllPopups();
        toggleClass(findAside(), "wf-is-forced");
    }
    else if (event.target.matches(".wf-popup-toggle"))
    {
        // Toggle other menu
        let popup = resolveTarget(event.target);
        if (popup && popup.matches(".wf-menu, .wf-dialog"))
            openPopup(popup);
    }
    else if (event.target.matches(".wf-menu-background, aside .wf-button, .wf-menu .wf-button"))
    {
        // Close all popups
        removeClass(findAside(), "wf-is-forced");
        closeAllPopups();
    }
    else if (event.target.matches(".wf-image"))
    {
        // Toggle image fullscreen
        if (event.target.matches(".wf-fullscreen"))
        {
            // Remove fullscreen image
            event.target.remove();
        }
        else
        {
            // Add fullscreen image
            let fullscreenImage = event.target.cloneNode();
            fullscreenImage.classList.add("wf-fullscreen");
            document.body.append(fullscreenImage);
        }
    }
});

document.addEventListener("submit", event =>
{
    if (watcherId && event.submitter.matches(".wf-server-form-override"))
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
    let request = new XMLHttpRequest();
    request.open("POST", `/wf/dyn/submit?id=${watcherId}&path=${encodeURIComponent(JSON.stringify(getSystemPath(submitter)))}`);
    request.onload = () =>
    {
        let action = JSON.parse(request.responseText);
        switch (action.type)
        {
            case "Nothing":
                break;
            case "Navigate":
                window.location.assign(action.location);
                break;
            default:
                console.warn("Unknown action", action);
                break;
        }
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