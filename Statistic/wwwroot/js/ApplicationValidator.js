function CheckName() {
    var name = document.getElementById("applicationName").value;
    if (name === "")
        return false;
    return true;
}