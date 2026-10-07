import { useEffect, useMemo, useState } from "react";
import { Activity, AlertTriangle, Gauge, MapPin, Radio, TrainFront, Wifi, WifiOff } from "lucide-react";
import { createTrainConnection, getApiUrl } from "./services/signalRService";

const statusMeta = {
  0: { label: "RUNNING", className: "running" },
  1: { label: "DELAYED", className: "delayed" },
  2: { label: "STOPPED", className: "stopped" },
  Running: { label: "RUNNING", className: "running" },
  Delayed: { label: "DELAYED", className: "delayed" },
  Stopped: { label: "STOPPED", className: "stopped" }
};

function App() {
  const [trains, setTrains] = useState([]);
  const [stats, setStats] = useState({ total: 0, running: 0, delayed: 0, stopped: 0, averageSpeed: 0 });
  const [connected, setConnected] = useState(false);
  const [lastUpdate, setLastUpdate] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const connection = createTrainConnection();

    connection.on("TrainUpdates", (updates) => {
      setTrains(updates);
      setLastUpdate(new Date());
    });

    connection.on("DashboardStats", (dashboardStats) => {
      setStats(dashboardStats);
    });

    connection.on("ConnectionStatus", () => setConnected(true));

    connection.onreconnecting(() => setConnected(false));
    connection.onreconnected(() => setConnected(true));
    connection.onclose(() => setConnected(false));

    fetch(`${getApiUrl()}/api/train`)
      .then((response) => response.ok ? response.json() : Promise.reject(new Error("API request failed")))
      .then((data) => setTrains(data))
      .catch((err) => setError(`Unable to load train data: ${err.message}`));

    connection.start()
      .then(() => setConnected(true))
      .catch((err) => {
        console.error("SignalR connection failed:", err);
        setConnected(false);
      });

    return () => {
      connection.stop();
    };
  }, []);

  const alerts = useMemo(
    () => trains.filter((train) => train.status !== 0 && train.status !== "Running").slice(0, 4),
    [trains]
  );

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand">
          <div className="brand-icon"><TrainFront size={24} /></div>
          <div>
            <h1>Railway Control Center</h1>
            <p>Real-time traffic monitoring</p>
          </div>
        </div>
        <div className={`connection ${connected ? "online" : "offline"}`}>
          {connected ? <Wifi size={16} /> : <WifiOff size={16} />}
          {connected ? "LIVE" : "OFFLINE"}
        </div>
      </header>

      <main className="content">
        <section className="hero">
          <div>
            <span className="eyebrow"><Radio size={15} /> LIVE OPERATIONS</span>
            <h2>Network Overview</h2>
            <p>Monitoring train movement and operational status in real time.</p>
          </div>
          <div className="updated">
            Last update
            <strong>{lastUpdate ? lastUpdate.toLocaleTimeString() : "Waiting..."}</strong>
          </div>
        </section>

        {error && <div className="error-banner"><AlertTriangle size={18} /> {error}</div>}

        <section className="stats-grid">
          <StatCard title="Total Trains" value={stats.total} icon={<TrainFront />} />
          <StatCard title="Running" value={stats.running} icon={<Activity />} accent="green" />
          <StatCard title="Delayed" value={stats.delayed} icon={<AlertTriangle />} accent="amber" />
          <StatCard title="Avg. Speed" value={`${stats.averageSpeed} km/h`} icon={<Gauge />} />
        </section>

        <div className="section-heading">
          <div>
            <h3>Live Train Fleet</h3>
            <span>{trains.length} active fleet records</span>
          </div>
          <span className="live-dot"><i /> SignalR streaming</span>
        </div>

        <section className="train-grid">
          {trains.map((train) => (
            <TrainCard key={train.id} train={train} />
          ))}
        </section>

        <section className="bottom-grid">
          <div className="panel">
            <div className="panel-heading">
              <h3>Operational Alerts</h3>
              <span>{alerts.length} active</span>
            </div>
            {alerts.length === 0 ? (
              <p className="empty">No operational alerts.</p>
            ) : (
              alerts.map((train) => (
                <div className="alert-row" key={train.id}>
                  <div className={`alert-icon ${(typeof train.status === "string" ? train.status : ["running","delayed","stopped"][train.status])}`}><AlertTriangle size={17} /></div>
                  <div>
                    <strong>{train.trainNumber} — {statusMeta[train.status].label}</strong>
                    <p>{train.delayMinutes > 0 ? `${train.delayMinutes} min delay` : "Train currently stopped"}</p>
                  </div>
                </div>
              ))
            )}
          </div>

          <div className="panel system-panel">
            <div className="panel-heading">
              <h3>System Status</h3>
              <span className="healthy">Healthy</span>
            </div>
            <div className="system-row"><span>API</span><strong>Operational</strong></div>
            <div className="system-row"><span>SignalR Hub</span><strong>{connected ? "Connected" : "Disconnected"}</strong></div>
            <div className="system-row"><span>Simulator</span><strong>Running</strong></div>
          </div>
        </section>
      </main>
    </div>
  );
}

function StatCard({ title, value, icon, accent = "" }) {
  return (
    <div className="stat-card">
      <div className={`stat-icon ${accent}`}>{icon}</div>
      <div>
        <span>{title}</span>
        <strong>{value}</strong>
      </div>
    </div>
  );
}

function TrainCard({ train }) {
  const meta = statusMeta[train.status];
  return (
    <article className="train-card">
      <div className="train-card-top">
        <div>
          <span className="train-number">{train.trainNumber}</span>
          <h3>{train.name}</h3>
        </div>
        <span className={`status ${meta.className}`}><i /> {meta.label}</span>
      </div>

      <div className="route">{train.route}</div>

      <div className="station-row">
        <div><MapPin size={17} /><div><span>Current</span><strong>{train.currentStation}</strong></div></div>
        <div className="next-station"><span>Next station</span><strong>{train.nextStation}</strong></div>
      </div>

      <div className="progress-track">
        <div className="progress-value" style={{ width: `${train.progressPercent}%` }} />
      </div>

      <div className="metrics">
        <div><span>Speed</span><strong>{Math.round(train.speedKmph)} km/h</strong></div>
        <div><span>Delay</span><strong className={train.delayMinutes > 0 ? "warning" : ""}>{train.delayMinutes} min</strong></div>
        <div><span>Progress</span><strong>{Math.round(train.progressPercent)}%</strong></div>
      </div>
    </article>
  );
}

export default App;
