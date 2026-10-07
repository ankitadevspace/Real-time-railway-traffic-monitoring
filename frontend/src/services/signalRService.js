import * as signalR from "@microsoft/signalr";

const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5078";

export const createTrainConnection = () =>
  new signalR.HubConnectionBuilder()
    .withUrl(`${API_URL}/hubs/train`, {
      transport: signalR.HttpTransportType.WebSockets,
      skipNegotiation: true
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build();

export const getApiUrl = () => API_URL;
