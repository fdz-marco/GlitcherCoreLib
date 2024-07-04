/*
Author: Marco Fernandez
Author URI: https://glitcher.dev
Version: 2024.07.03
License: MIT License
*/

const chatMessages = document.getElementById('chat-messages');
const messageInput = document.getElementById('message-input');
const sendButton = document.getElementById('send-button');
const toggleModeButton = document.getElementById('toggle-mode');
const websocketInput = document.getElementById('websocket-input');
const websocketButton = document.getElementById('websocket-button');
const templateButton = document.getElementById('template-button');
const templateDropdown = document.getElementById('template-dropdown');
const resizeHandle = document.getElementById('resize-handle');
const inputArea = document.getElementById('input-area');
const chatContainer = document.getElementById('chat-container');

let socket;
//let websocketUrl = 'wss://echo.websocket.org';
let websocketUrl = 'http://localhost:8081/';

/* ======================================= */
/* Window On Load */

window.onload = (event) => {
  updateWebsocketInfo(false);
  websocketInput.value = websocketUrl;
  toggleModeButton.onclick = toggleMode;
  websocketButton.onclick = toggleWebsocket;
  sendButton.onclick = sendMessage;
  messageInput.oninput = updateSyntaxHighlighting;
  messageInput.onkeydown = function(e) {
    if (e.key === 'Enter' && e.ctrlKey) {
      sendMessage();
      e.preventDefault();
    }
    };
    //templateDropdown.innerHTML = '';    
    //createTemplate("test", `{"apiKey" : "", "action" : "test", "payload" : "" }`);  
};

/* ======================================= */
/* Dark Mode */

function toggleMode() {
  document.body.classList.toggle('dark-mode');
}

/* ======================================= */
/* WebSocket Functions */

function updateWebsocketInfo(connected) {
  if (connected) {
    websocketInput.value = websocketUrl;
    websocketInput.disabled = true;
    websocketButton.textContent = 'Disconnect';
  } else {
    //websocketInput.value = '';
    websocketInput.disabled = false;
    websocketButton.textContent = 'Connect';
  }
}

function toggleWebsocket() {
  if (socket && socket.readyState === WebSocket.OPEN) {
    socket.close();
  } else {
    const newUrl = websocketInput.value;
    if (newUrl) {
      websocketUrl = newUrl;
      connect();
    }
  }
}

function connect() {
  socket = new WebSocket(websocketUrl);

  socket.onopen = function(e) {
    updateWebsocketInfo(true);
    addSystemMessage('Connected to WebSocket');
  };

  socket.onmessage = function(event) {
    addMessage(event.data, 'received');
  };

  socket.onclose = function(event) {
    updateWebsocketInfo(false);
    addSystemMessage('Disconnected from WebSocket');
  };

  socket.onerror = function(error) {
    console.log(`WebSocket Error: ${error}`);
    addSystemMessage('WebSocket Error occurred');
  };
}

function sendMessage() {
  const message = messageInput.innerText;
  if (message && socket && socket.readyState === WebSocket.OPEN) {
    socket.send(message);
    addMessage(message, 'sent');
    messageInput.innerHTML = '';
    updateSyntaxHighlighting();
  }
}

/* ======================================= */
/* Chat Messages */

function addSystemMessage(message) {
  const messageElement = document.createElement('div');
  messageElement.className = 'message system';
  messageElement.textContent = message;
  chatMessages.appendChild(messageElement);
  chatMessages.scrollTop = chatMessages.scrollHeight;
}

function addMessage(message, type) {
  const messageElement = document.createElement('div');
  messageElement.className = `message ${type}`;
  
  const messageContent = document.createElement('pre');
  try {
    const jsonObj = JSON.parse(message);
    messageContent.innerHTML = syntaxHighlight(jsonObj);
  } catch (e) {
    messageContent.textContent = message;
  }
  messageElement.appendChild(messageContent);
  
  const timestamp = document.createElement('div');
  timestamp.className = 'timestamp';
  timestamp.textContent = new Date().toLocaleString();
  messageElement.appendChild(timestamp);
  
  chatMessages.appendChild(messageElement);
  chatMessages.scrollTop = chatMessages.scrollHeight;
}

/* ======================================= */
/* JSON Syntax Highlighting */

function updateSyntaxHighlighting() {
  // ToDo / WIP: Mantain cursor on the position
  const selection = window.getSelection();
  const parentNode = (selection.baseNode != null) ? selection.baseNode.parentNode : null;
  const cursorPosition = selection.anchorOffset;
  const type = selection.anchorNode;

  console.error(selection);
  console.warn(parentNode);
  console.info(cursorPosition);
  console.warn(type);


  //const range = (selection.rangeCount > 0) ? selection.getRangeAt(0) : null;
  //const startContainer = (range != null) ? range.startContainer : parentNode;
  //const startOffset = (range != null) ? range.startOffset : selection.lenght;
  
  //console.log(selection);
  //const container = selection.baseNode.parentNode;
  //console.log(`${selection}|${range}|${startOffset}|${startContainer}|`);
  //console.log(startContainer);

  let content = messageInput.innerText;
  //console.log(`${content}`);

  try {
    const jsonObj = JSON.parse(content);
    content = JSON.stringify(jsonObj, null, 2);
    messageInput.innerHTML = syntaxHighlight(content);
  } catch (e) {
    // Not valid JSON, do nothing
    messageInput.innerHTML = content;
  }

  // Restore cursor position
  const newRange = document.createRange();
  newRange.setStart(type, 0);
  newRange.collapse(true);
  selection.removeAllRanges();
  selection.addRange(newRange);

  /*if (startContainer.nodeType === Node.TEXT_NODE) {
  }*/
}

function syntaxHighlight(json) {
  if (typeof json != 'string') {
    json = JSON.stringify(json, undefined, 2);
  }
  json = json.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  return json.replace(/("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g, function (match) {
    var cls = 'json-number';
    if (/^"/.test(match)) {
      if (/:$/.test(match)) {
        cls = 'json-key';
      } else {
        cls = 'json-string';
      }
    } else if (/true|false/.test(match)) {
      cls = 'json-boolean';
    } else if (/null/.test(match)) {
      cls = 'json-null';
    }
    return '<span class="' + cls + '">' + match + '</span>';
  });
}

/* ======================================= */
/* Templates Dropdown Menu */

templateButton.onclick = function() {
  templateDropdown.style.display = templateDropdown.style.display === 'block' ? 'none' : 'block';
};

document.querySelectorAll('#template-dropdown a').forEach(item => {
  item.addEventListener('click', event => {
    event.preventDefault();
    messageInput.innerText = item.getAttribute('data-template');
    updateSyntaxHighlighting();
    templateDropdown.style.display = 'none';
  });
});

window.onclick = function(event) {
  if (!event.target.matches('#template-button')) {
    templateDropdown.style.display = 'none';
  }
}

function createTemplate(templateName, templateData) {
    document.createElement('a');
    const templateLink = document.createElement('a');
    templateLink.setAttribute('data-template', templateData);
    templateLink.textContent = templateName;
    templateLink.addEventListener('click', event => {
        event.preventDefault();
        messageInput.innerText = templateLink.getAttribute('data-template');
        updateSyntaxHighlighting();
        templateDropdown.style.display = 'none';
    });
    templateDropdown.appendChild(templateLink);
}

/* ======================================= */
/* Horizontal Split View Resizing */

let isResizing = false;

resizeHandle.addEventListener('mousedown', (e) => {
  isResizing = true;
  document.addEventListener('mousemove', resize);
  document.addEventListener('mouseup', stopResize);
});

function resize(e) {
  if (!isResizing) return;
  const containerHeight = chatContainer.offsetHeight;
  const newMessagesHeight = e.clientY - chatContainer.offsetTop;
  const newInputHeight = containerHeight - newMessagesHeight - resizeHandle.offsetHeight;

  if (newMessagesHeight > 100 && newInputHeight > 100) {
    chatMessages.style.height = `${newMessagesHeight}px`;
    inputArea.style.flex = `0 0 ${newInputHeight}px`;
  }
}

function stopResize() {
  isResizing = false;
  document.removeEventListener('mousemove', resize);
  document.removeEventListener('mouseup', stopResize);
}

/* ======================================= */
