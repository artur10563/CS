

import { createPromiseClient } from "https://esm.sh/@bufbuild/connect@0.13.0?dev";
import { createGrpcWebTransport } from "https://esm.sh/@bufbuild/connect-web@0.13.0?dev";
import { proto3, MethodKind } from "https://esm.sh/@bufbuild/protobuf@1.7.2?dev";

const int32TypeId = 5;
const stringTypeId = 9;

// --------- Proto definitions START ---------
const Room = proto3.makeMessageType("chat.Room", [
    { no: 1, name: "id", kind: "scalar", T: int32TypeId },
    { no: 2, name: "name", kind: "scalar", T: stringTypeId },
]);

const RoomCreatedResponse = proto3.makeMessageType("chat.RoomCreatedResponse", [
    { no: 1, name: "room", kind: "message", T: Room },
]);

const JoinRoomRequest = proto3.makeMessageType("chat.JoinRoomRequest", [
    { no: 1, name: "room_id", jsonName: "roomId", kind: "scalar", T: int32TypeId },
    { no: 2, name: "user_name", jsonName: "userName", kind: "scalar", T: stringTypeId },
]);

const LeaveRoomRequest = proto3.makeMessageType("chat.LeaveRoomRequest", [
    { no: 1, name: "room_id", jsonName: "roomId", kind: "scalar", T: int32TypeId },
    { no: 2, name: "user_name", jsonName: "userName", kind: "scalar", T: stringTypeId },
]);

const MessageRequest = proto3.makeMessageType("chat.MessageRequest", [
    { no: 1, name: "room_id", jsonName: "roomId", kind: "scalar", T: int32TypeId },
    { no: 2, name: "user_name", jsonName: "userName", kind: "scalar", T: stringTypeId },
    { no: 3, name: "message", kind: "scalar", T: stringTypeId },
]);

const ChatMessage = proto3.makeMessageType("chat.ChatMessage", [
    { no: 1, name: "room_id", jsonName: "roomId", kind: "scalar", T: int32TypeId },
    { no: 2, name: "user_name", jsonName: "userName", kind: "scalar", T: stringTypeId },
    { no: 3, name: "message", kind: "scalar", T: stringTypeId },
    { no: 4, name: "timestamp", kind: "scalar", T: stringTypeId },
]);

const Empty = proto3.makeMessageType("chat.Empty", []);

const ChatService = {
    typeName: "chat.ChatService",
    methods: {
        createNewRoom: { name: "CreateNewRoom", I: Room, O: RoomCreatedResponse, kind: MethodKind.Unary },
        joinRoom: { name: "JoinRoom", I: JoinRoomRequest, O: ChatMessage, kind: MethodKind.ServerStreaming },
        leaveRoom: { name: "LeaveRoom", I: LeaveRoomRequest, O: Empty, kind: MethodKind.Unary },
        sendMessage: { name: "SendMessage", I: MessageRequest, O: Empty, kind: MethodKind.Unary },
    },
};

// --------- Proto definitions END---------
let client;
let currentStreamAbort;
let currentRoomId = null;
let currentUser = null;

const els = {};

document.addEventListener("DOMContentLoaded", () => {
    els.baseUrl = document.getElementById("baseUrl");
    els.userName = document.getElementById("userName");
    els.roomName = document.getElementById("roomName");
    els.roomId = document.getElementById("roomId");
    els.messageInput = document.getElementById("messageInput");
    els.createRoomBtn = document.getElementById("createRoomBtn");
    els.joinBtn = document.getElementById("joinBtn");
    els.leaveBtn = document.getElementById("leaveBtn");
    els.sendBtn = document.getElementById("sendBtn");
    els.status = document.getElementById("status");
    els.messages = document.getElementById("messages");

    els.createRoomBtn.addEventListener("click", handleCreateRoom);
    els.joinBtn.addEventListener("click", handleJoin);
    els.leaveBtn.addEventListener("click", handleLeave);
    els.sendBtn.addEventListener("click", handleSend);

    buildClient();
});

function buildClient() {
    const baseUrl = els.baseUrl.value.trim() || "http://localhost:5250";

    const transport = createGrpcWebTransport({
        baseUrl,
        useBinaryFormat: true,
    });
    client = createPromiseClient(ChatService, transport);
    setStatus(`Ready. Base URL: ${baseUrl}`);
}

async function handleCreateRoom() {
    try {
        const name = els.roomName.value.trim() || "New room";
        const res = await client.createNewRoom({ name });
        const newId = res.room?.id;
        appendSystem(`Room created: #${newId} (${res.room?.name ?? name})`);
        if (newId) {
            els.roomId.value = newId;
        }
    } catch (err) {
        reportError("Create room failed", err);
    }
}

async function handleJoin() {
    const roomId = parseInt(els.roomId.value, 10);
    const userName = els.userName.value.trim();
    if (!Number.isInteger(roomId) || roomId <= 0) {
        appendSystem("Room id must be a positive number.");
        return;
    }
    if (!userName) {
        appendSystem("Please enter a user name before joining.");
        return;
    }

    await stopStreaming();
    buildClient();

    currentRoomId = roomId;
    currentUser = userName;
    setStatus(`Connecting to room #${roomId} as ${userName}...`);

    currentStreamAbort = new AbortController();
    try {
        const stream = client.joinRoom(
            { roomId, userName },
            { signal: currentStreamAbort.signal }
        );

        appendSystem(`Joined room #${roomId} as ${userName}`);
        setStatus(`Listening for messages in room #${roomId}`);

        for await (const msg of stream) {
            appendMessage(msg);
        }
    } catch (err) {
        if (err.name === "AbortError") {
            appendSystem("Streaming stopped.");
            return;
        }
        reportError("Join stream ended with error", err);
    } finally {
        await handleLeave(false);
    }
}

async function handleLeave(notifyServer = true) {
    if (!currentRoomId || !currentUser) {
        setStatus("Not connected");
        return;
    }

    await stopStreaming();

    if (notifyServer) {
        try {
            await client.leaveRoom({
                roomId: currentRoomId,
                userName: currentUser,
            });
            appendSystem(`Left room #${currentRoomId}`);
        } catch (err) {
            reportError("Leave failed", err);
        }
    }

    currentRoomId = null;
    currentUser = null;
    setStatus("Not connected");
}

async function handleSend() {
    if (!currentRoomId || !currentUser) {
        appendSystem("Join a room before sending messages.");
        return;
    }
    const text = els.messageInput.value.trim();
    if (!text) return;

    try {
        await client.sendMessage({
            roomId: currentRoomId,
            userName: currentUser,
            message: text,
        });
        els.messageInput.value = "";
    } catch (err) {
        reportError("Send failed", err);
    }
}

async function stopStreaming() {
    if (currentStreamAbort) {
        currentStreamAbort.abort();
        currentStreamAbort = null;

        await new Promise((r) => setTimeout(r, 10));
    }
}

function appendMessage(msg) {
    const li = document.createElement("li");
    const ts = msg.timestamp ? new Date(msg.timestamp).toLocaleTimeString() : "";
    li.innerHTML = `
        <div class="meta">#${msg.roomId} • ${msg.userName} ${ts ? `@ ${ts}` : ""}</div>
        <div>${escapeHtml(msg.message || "")}</div>
    `;
    els.messages.appendChild(li);
    els.messages.scrollTop = els.messages.scrollHeight;
}

function appendSystem(text) {
    const li = document.createElement("li");
    li.innerHTML = `<div class="meta">System</div><div>${escapeHtml(text)}</div>`;
    els.messages.appendChild(li);
    els.messages.scrollTop = els.messages.scrollHeight;
}

function setStatus(text) {
    els.status.textContent = text;
}

function reportError(label, err) {
    console.error(label, err);
    appendSystem(`${label}: ${err?.message ?? err}`);
}

function escapeHtml(str) {
    return str.replace(/[&<>"']/g, (c) => ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;",
    }[c]));
}