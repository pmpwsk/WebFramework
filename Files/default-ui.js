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
            case "Welcome":
                watcherId = change.id;
                break;
            case "Head":
                document.head.innerHTML = "";
                for (let html of change.elements)
                    document.head.append(parseElement(html));
                break;
            case "BodyBeforeScript":
                for (let child of [...document.body.children])
                    if (matchesSystemId(child, "script"))
                        break;
                    else
                        child.remove();
                for (let html of change.elements.reverse())
                    document.body.prepend(parseElement(html));
                break;
            case "BodyAfterScript":
                let scriptPassed = false;
                for (let child of [...document.body.children])
                    if (matchesSystemId(child, "script"))
                        scriptPassed = true;
                    else if (scriptPassed)
                        child.remove();
                for (let html of change.elements)
                    document.body.append(parseElement(html));
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
        closeAllMenus();
        toggleClass(findAside(), "wf-is-forced");
    }
    else if (event.target.matches(".wf-menu-toggle"))
    {
        // Toggle other menu
        let menu = resolveTarget(event.target);
        if (menu && menu.matches(".wf-menu"))
            openMenu(menu);
    }
    else if (event.target.matches(".wf-overlay-background, aside *, .wf-menu *"))
    {
        // Close all menus
        removeClass(findAside(), "wf-is-forced");
        closeAllMenus();
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

function findAside()
{
    return document.querySelector("aside");
}

function openMenu(menu)
{
    removeClass(findAside(), "wf-is-forced");
    closeAllMenus(menu);
    toggleClass(menu, "wf-is-open");
}

function closeAllMenus(except)
{
    for (let menu of document.querySelectorAll(".wf-menu"))
        if (!except || menu !== except)
            removeClass(menu, "wf-is-open");
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
            {
                console.warn("Unknown action", action);
            } break;
        }
    }
    request.send(new FormData(form));
}

function parseElement(html)
{
    let template = document.createElement("template");
    template.innerHTML = html;
    return template.content.firstChild;
}