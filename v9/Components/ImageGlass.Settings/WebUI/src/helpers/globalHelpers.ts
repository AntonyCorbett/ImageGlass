import { pause } from '.';
import { WebviewEventHandlerFn } from './webview';


/**
 * Gets the first matched element with the query selector.
 */
export const query = <T = HTMLElement>(selector: string): T | null => {
  try {
    return document.querySelector(selector) as T;
  }
  catch {}

  return null;
};


/**
 * Gets all matched elements with the query selector.
 */
export const queryAll = <T = HTMLElement>(selector: string) => {
  try {
    return Array.from(document.querySelectorAll(selector)) as T[];
  }
  catch {}

  return [];
};


/**
 * Add event listerner from backend.
 * @param name Event name
 * @param handler Function to handle the event
 */
export const on = (name: string, handler: WebviewEventHandlerFn) => {
  _webview.addEvent(name, handler);
};


/**
 * Send an event to backend.
 * @param name Event name
 * @param data Data to send to backend, it will be converted to JSON string.
 */
export const post = (name: string, data?: any) => {
  // @ts-ignore
  window.chrome.webview?.postMessage({ name, data: JSON.stringify(data) });
};


/**
 * Send an event to backend and wait for the returned data.
 * @param name Event name
 * @param data Data to send to backend, it will be converted to JSON string.
 */
export const postAsync = async <T = unknown>(name: string, data?: any) => {
  let hasResult = false;
  let result: T = null;

  on(name, (eventName, eventData) => {
    if (eventName !== name) return;

    hasResult = true;
    result = eventData;
    _webview.removeEvent(name);
  });

  // @ts-ignore
  window.chrome.webview?.postMessage({ name, data: JSON.stringify(data) });

  // wait for the returned data
  while (!hasResult) {
    await pause(100);
  }

  return result;
};
