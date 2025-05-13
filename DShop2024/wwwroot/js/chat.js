"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();


//document.getElementById("sendMessageButton").disabled = true;

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
    var li = document.createElement("li");
    var span = document.createElement("span");
    var p = document.createElement("p");

    if (user == message["userName"] && message["receiver"] == null) {
        var idmessage = `messagesList_${message["userName"]}`;
        document.getElementById(idmessage).appendChild(li);
        li.classList.add("text-info");
        li.appendChild(span);
        li.appendChild(p);
        span.textContent = `${message["timestamp"]}`;
        p.textContent = `[${message["roleName"]}][${user}] : ${message["contentMessage"]}`;


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