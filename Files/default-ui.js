let aside = document.querySelector("aside");
if (document.documentElement.hasAttribute("data-wf-watcher"))
{
    let watcherId = document.documentElement.getAttribute("data-wf-watcher");
    let watcher = new EventSource(`/wf/dyn/watcher?id=${watcherId}`);
    let watcherGreeted = false;
    onbeforeunload = () => watcher.close();
    watcher.onmessage = async event =>
    {
        if (event.data.startsWith(":"))
            return;
        
        let change = JSON.parse(event.data);
        switch (change.type)
        {
            case "Reload":
                window.location.reload();
                break;
            case "Welcome":
                watcherGreeted = true;
                break;
            case "WelcomeBack":
                if (!watcherGreeted)
                    window.location.reload();
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
        toggleClass(aside, "wf-is-forced");
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
        removeClass(aside, "wf-is-forced");
        closeAllMenus();
    }
    else if (event.target.matches(".wf-image"))
    {
        // Toggle image fullscreen
        toggleClass(event.target, "wf-fullscreen");
    }
});

function openMenu(menu)
{
    removeClass(aside, "wf-is-forced");
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

function getElementBySystemId(parent, id)
{
    for (let child of parent.children)
        if (child.hasAttribute("data-wf-id") && child.getAttribute("data-wf-id") === id)
            return child;
    return null;
}

function parseElement(html)
{
    let template = document.createElement("template");
    template.innerHTML = html;
    return template.content.firstChild;
}