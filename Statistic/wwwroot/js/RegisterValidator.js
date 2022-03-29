function PaswordMatch() {
    var password = document.getElementById("registerPassword").value;
    var verifyPassword = document.getElementById("registerVerifyPassword").value;
    if (password === "" || verifyPassword === "") {
        document.getElementById("passwordEmptyError").style.display = "inline";
        return false;
    }
        if (password == verifyPassword) {
        document.getElementById("passwordError").style.display = "none";
        return true;
    } else {
        document.getElementById("passwordError").style.display = "inline";
        return false;
    }
}
function ValidEmail() {
    var email = document.getElementById("registerEmail").value;
    if (email === "") {
        document.getElementById("emailError").style.display = "inline";
        return false;
    }
    if (String(email)
            .toLowerCase()
            .match(
                /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
        )) {
            document.getElementById("emailError").style.display = "none";
            return true;
        } else {
            document.getElementById("emailError").style.display = "inline";
            return false;
        }
    
}
function SubmitCheck() {
    return ValidEmail() && PaswordMatch();
}