function HideParent(elem) {
    if (elem) {
        var parelem = elem.parentElement;
        console.log(parelem);
        if (parelem) {
            parelem.classList.add("Hidden");
        }
    }
}

function ShowShroud() {
    document.getElementById("divShroud").classList.remove("Hidden");
}