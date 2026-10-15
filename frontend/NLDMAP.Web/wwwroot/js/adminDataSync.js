// Persist only an opaque revision. No account data or credentials enter browser storage.
const storageKey = "nldmap.admin.data.revision.v1";
const channelName = "nldmap.admin.data.v1";
let receiver;
let channel;
let revision = "initial";
let storageAvailable = false;

function readStorage() {
    if (!storageAvailable) return null;
    try { return globalThis.localStorage.getItem(storageKey); }
    catch { storageAvailable = false; return null; }
}

function signal(next) {
    if (typeof next !== "string" || next.length > 100 || next === revision) return;
    revision = next;
    receiver?.invokeMethodAsync("OnBrowserDataChanged").catch(() => {});
}

function onStorage(event) {
    if (event.key === storageKey) signal(event.newValue ?? "initial");
}

function onFocus() {
    const next = readStorage();
    if (next !== null) signal(next);
}

function onVisibility() {
    if (document.visibilityState === "visible") onFocus();
}

export function start(dotnet) {
    stop();
    receiver = dotnet;
    try {
        const probe = storageKey + ".probe";
        globalThis.localStorage.setItem(probe, "1");
        globalThis.localStorage.removeItem(probe);
        storageAvailable = true;
    } catch { storageAvailable = false; }
    revision = readStorage() ?? "initial";
    try {
        channel = new BroadcastChannel(channelName);
        channel.onmessage = event => signal(event.data);
    } catch { channel = undefined; }
    globalThis.addEventListener("storage", onStorage);
    globalThis.addEventListener("focus", onFocus);
    document.addEventListener("visibilitychange", onVisibility);
    return storageAvailable || !!channel;
}

export function getRevision() {
    const stored = readStorage();
    if (!storageAvailable && !channel) throw new Error("Data coordination unavailable");
    return stored ?? revision;
}

export function publish() {
    revision = globalThis.crypto.randomUUID();
    if (storageAvailable) {
        try { globalThis.localStorage.setItem(storageKey, revision); }
        catch { storageAvailable = false; }
    }
    channel?.postMessage(revision);
    return revision;
}

export function stop() {
    globalThis.removeEventListener("storage", onStorage);
    globalThis.removeEventListener("focus", onFocus);
    document.removeEventListener("visibilitychange", onVisibility);
    channel?.close();
    channel = undefined;
    receiver = undefined;
}
