"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();


var buttonSend = document.getElementById("sendMessageButton")
if (buttonSend != null) {
    buttonSend.disabled = true;
}

//ReceiveMessage
connection.on("ReceiveMessage", function (user, message) {
    //contentMessage
    //timestamp
    //userName
    //roleName
    //receiver

    if (user == message["userName"] && message["receiver"] == null) {

        var spanIdU = `spanuser_${message["userName"]}`;
        var spannoti = `spanNoti`;

        var divideuser = document.getElementById(spanIdU);
        divideuser.style.display = "block";

        var dividenoti = document.getElementById(spannoti);
        dividenoti.style.display = "block";
        //if (divideuser.style.display == "none") {
        //    console.log("2a");
        //    divideuser.style.display = "block";
        //} else {
        //    console.log("2b" + divideuser.style.display);
        //    divideuser.style.display = "block";
        //}
    
    } else if (user == message["userName"] && message["receiver"] != "") {
        var idmessage = `messagesList_${message["receiver"]}`;
        document.getElementById(idmessage).appendChild(li);
        li.appendChild(span);
        li.appendChild(p)
        span.textContent = `${message["timestamp"]}`;
        p.textContent = `[${message["roleName"]}][${user}] : ${message["contentMessage"]}`;
    }
       
});



//create and connect
connection.start().then(function () {
    //document.getElementById("sendMessageButton").disabled = false;
    var bSend = document.getElementById("sendMessageButton")
    if (bSend != null) {
        bSend.disabled = false;
    }
}).catch(function (err) {
    return console.error(err.toString());
});

//Send message
//document.getElementById("sendMessageButton").addEventListener("click", function (event) {
//    var user = document.getElementById("userInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});