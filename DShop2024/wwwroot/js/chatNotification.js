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
    var linkA = document.createElement("a");
    var divImg = document.createElement("div");
    var divContent = document.createElement("div");
    var avatar = document.createElement("img");
    var spanUserName = document.createElement("span");
    var strongUserName = document.createElement("strong");
    var spanMessage = document.createElement("span");
    var spanTime = document.createElement("span");


    if (user == message["userName"] && message["receiver"] == null) {
        var idUserMessage = `notiMessage_${message["userName"]}`;
        var userMessage = document.getElementById(idUserMessage);

        var contentMessage = `${message["contentMessage"]}`;
        if (contentMessage.length > 10) {
            contentMessage = str.substring(0, 10);
        }

        if (userMessage != null) {
            var idNotiContentMessage = `notiContentMessage_${message["userName"]}`;
            var notiContentMessage = document.getElementById(idNotiContentMessage);
            notiContentMessage.textContent = contentMessage

            var idNotiRimespan = `notiTimespan_${message["userName"]}`;
            var notiRimespan = document.getElementById(idNotiRimespan);
            notiRimespan.textContent = `${message["daysLeftTime"]}`
        }
        else {
            var idnoti = `listNotiMessage`;
            var listNotiM = document.getElementById(idnoti);
            listNotiM.appendChild(linkA);
            linkA.href = `${message["pathUser"]}`;
            linkA.id = `notiMessage_${message["userName"]}`;
            linkA.appendChild(divImg);
            divImg.classList.add("notif-img");
            divImg.appendChild(avatar)
            avatar.src = `${message["pathImage"]}`;

            linkA.appendChild(divContent);
            divContent.classList.add("notif-content");
            divContent.appendChild(spanUserName);
            spanUserName.classList.add("subject");
            spanUserName.appendChild(strongUserName);
            strongUserName.textContent = `${message["userName"]}`;

            divContent.appendChild(spanMessage);
            spanMessage.classList.add("block");
            spanMessage.textContent = contentMessage;
            spanMessage.id = `notiContentMessage_${message["userName"]}`;

            divContent.appendChild(spanTime);
            spanTime.classList.add("time");
            spanTime.textContent = `${message["daysLeftTime"]}`;
            spanTime.id = `notiTimespan_${message["userName"]}`;

            var countNotiM = document.getElementById(`countNotiMessage`);
            if (countNotiM.textContent == "") {
                countNotiM.classList.add("notification");
                countNotiM.textContent = `1`;
            } else {
                countNotiM.textContent = parseInt(countNotiM.textContent) + 1;
            }
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

//Send message
//document.getElementById("sendMessageButton").addEventListener("click", function (event) {
//    var user = document.getElementById("userInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});