"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();


//document.getElementById("sendMessageButton").disabled = true;

var buttonSend = document.getElementById("sendMessageButton")
if (buttonSend != null) {
    buttonSend.disabled = true;
}


connection.on("ReceiveMessage", function (user, message) {
    //contentMessage
    //timestamp
    //userName
    //roleName
    //receiver

    //alert(Object.values(obj));
    //alert(Object.keys(obj));

    var divTime = document.createElement("div");
    var divContent = document.createElement("div");
    var divMessage = document.createElement("div");
    var pTime = document.createElement("p");
    var pSpace = document.createElement("p");
    var pUserName = document.createElement("p");
    var avatar = document.createElement("img");
    var pMessage = document.createElement("p");

    if (user == message["userName"] && message["receiver"] == null) {
        var idmessage = `messagesList_${message["userName"]}`;
        var divmessage = document.getElementById(idmessage);
        divmessage.appendChild(divTime);
        divTime.classList.add("styletime-customer"); 
        divTime.appendChild(pTime);
        pTime.textContent = `${message["timestamp"]}`;
        pTime.classList.add("sptime");
        divTime.appendChild(pSpace);
        pSpace.classList.add("sptime");
        divTime.appendChild(pUserName);
        pUserName.classList.add("sptime");    
        pSpace.textContent = `___`;
        pUserName.textContent = `[${message["roleName"]}]${user}`;

        divmessage.appendChild(divContent);
        divContent.classList.add("stylecontent-customer");
        divContent.appendChild(divMessage);
        divMessage.appendChild(pMessage);
        pMessage.classList.add("spcontent-customer");
        divContent.appendChild(avatar);
        avatar.classList.add("message-img");     
        avatar.src = `${message["pathImage"]}`;
        pMessage.innerHTML = `${message["contentMessage"]}`;
        divmessage.scrollTop = divmessage.scrollHeight;

    } else if (user == message["userName"] && message["receiver"] != "") {
        var idmessage = `messagesList_${message["receiver"]}`;
        var divmessage = document.getElementById(idmessage);
        divmessage.appendChild(divTime);
        divTime.classList.add("styletime-toreciever");
        divTime.appendChild(pTime);
        pTime.textContent = `${message["timestamp"]}`;
        pTime.classList.add("sptime");
        divTime.appendChild(pSpace);
        pSpace.classList.add("sptime");
        divTime.appendChild(pUserName);
        pUserName.classList.add("sptime");
        pSpace.textContent = `___`;
        pUserName.textContent = `[${message["roleName"]}]${user}`;

        divmessage.appendChild(divContent);
        divContent.appendChild(avatar);
        divContent.classList.add("stylecontent-toreciever");
        avatar.classList.add("message-img");
        avatar.src = `${message["pathImage"]}`;
        divContent.appendChild(divMessage);
        divMessage.appendChild(pMessage);
        pMessage.classList.add("spcontent-toreciever");
        pMessage.innerHTML = `${message["contentMessage"]}`;
        divmessage.scrollTop = divmessage.scrollHeight;
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

