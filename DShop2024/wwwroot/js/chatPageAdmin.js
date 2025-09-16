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

    var rowUser = document.createElement("tr");
    var tdImageAndUserName = document.createElement("td");
    var divUserName = document.createElement("div");
    var divImg = document.createElement("div");
    var avatar = document.createElement("img");

    var tdMessage = document.createElement("td");
    var divNewMessage = document.createElement("div");
    var spanTimestamp = document.createElement("span");
    var pContentMessage = document.createElement("p");

    var tdButtonChat = document.createElement("td");
    var aButtonChat = document.createElement("a");

    if (user == message["userName"] && message["receiver"] == null) {
        var tbodyTable = document.getElementById(`ChatWithCustomer`);

        var idRow = `row_${message["userName"]}`;
        var userMessage = document.getElementById(idRow);
        if (userMessage != null) {
            userMessage.remove();
        

            tbodyTable.prepend(rowUser);
            rowUser.id = idRow
            rowUser.appendChild(tdImageAndUserName);
            tdImageAndUserName.appendChild(divUserName);
            divUserName.classList.add("col-sm-12");
            divUserName.textContent = `${message["userName"]}`;
            tdImageAndUserName.appendChild(divImg);
            divImg.classList.add("col-sm-12");
            divImg.appendChild(avatar)
            divImg.classList.add("notif-img");
            avatar.src = `${message["pathImage"]}`;
            avatar.style.width = '100px';
            avatar.style.height = '100px';

            rowUser.appendChild(tdMessage);
            tdMessage.appendChild(divNewMessage);
            divNewMessage.classList.add("sNewMessage");
            divNewMessage.textContent = `New message`;
            divNewMessage.id = `snewMessage_${message["userName"]}`;
            tdMessage.appendChild(spanTimestamp);
            spanTimestamp.textContent = `${message["timeStamp"]}`;
            tdMessage.appendChild(pContentMessage);
            pContentMessage.textContent = `${message["contentMessage"]}`;

            rowUser.appendChild(tdButtonChat);
            tdButtonChat.appendChild(aButtonChat);
            aButtonChat.href = `${message["pathUser"]}`;
            aButtonChat.id = `chat_${message["userName"]}`;
            aButtonChat.classList.add("sendMessageButton");
            aButtonChat.textContent = "chat";
        } 
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

