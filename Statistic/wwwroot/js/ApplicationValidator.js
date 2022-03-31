function CheckName() {
    var name = document.getElementById("applicationName").value;
    if (name === "") {
        document.getElementById("applicationNameError").style.display = "inline";
        return false;
    }
    document.getElementById("applicationNameError").style.display = "hidden";
    return true;
}