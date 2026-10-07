export type SyncEventType = "AUTH_LOGOUT" | "TENANT_CHANGED" | "THEME_CHANGED";

export interface SyncMessage<T = unknown> {
  type: SyncEventType;
  payload?: T;
  timestamp: number;
}

type SyncCallback<T> = (message: SyncMessage<T>) => void;

class TabSyncService {
  private channel: BroadcastChannel | null = null;
  private readonly listeners = new Map<
    SyncEventType,
    Set<SyncCallback<unknown>>
  >();

  constructor() {
    if (typeof window !== "undefined" && "BroadcastChannel" in window) {
      try {
        this.channel = new BroadcastChannel("modular_saas_sync");
        this.channel.onmessage = (event: MessageEvent<SyncMessage>) => {
          this.notify(event.data);
        };
      } catch {
        this.channel = null;
      }
    }
  }

  broadcast<T = unknown>(type: SyncEventType, payload?: T): void {
    const message: SyncMessage<T> = {
      type,
      payload,
      timestamp: Date.now(),
    };

    if (this.channel) {
      try {
        this.channel.postMessage(message);
      } catch {
        // Fallback or ignore
      }
    }
  }

  on<T = unknown>(type: SyncEventType, callback: SyncCallback<T>): () => void {
    if (!this.listeners.has(type)) {
      this.listeners.set(type, new Set());
    }

    const set = this.listeners.get(type)!;
    set.add(callback as SyncCallback<unknown>);

    return () => {
      set.delete(callback as SyncCallback<unknown>);
    };
  }

  private notify(message: SyncMessage): void {
    const callbacks = this.listeners.get(message.type);
    if (callbacks) {
      callbacks.forEach((cb) => {
        try {
          cb(message);
        } catch {
          // Swallow callback exceptions
        }
      });
    }
  }
}

export const tabSyncService = new TabSyncService();
