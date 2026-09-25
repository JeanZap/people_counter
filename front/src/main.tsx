import { useQuery } from '@tanstack/react-query';
import { createRoot } from 'react-dom/client';
import { AppProviders } from './App.Providers';
import { Camera, DetectionEvent } from 'domain/types';
import { usePeopleCounterService } from 'services/peopleCounter.service';
import './style.css';

function App() {
	const service = usePeopleCounterService();
	const cameras = useQuery<Camera[]>({ queryKey: ['cameras'], queryFn: service.listarCameras });
	const events = useQuery<DetectionEvent[]>({ queryKey: ['events'], queryFn: service.listarEventos });
	const cameraItems = cameras.data ?? [];
	const eventItems = events.data ?? [];
	const apiUrl = import.meta.env.VITE_BASE_API_URL ?? 'http://localhost:8080/api/v1';
	return <main><header><div><span className="eyebrow">LOCAL VISION / MONITORING</span><h1>People Counter</h1><p>Eventos detectados e gravações da operação local.</p></div><span className="pill">● Sistema operacional</span></header><section className="stats"><div><small>CÂMERAS</small><strong>{cameraItems.length}</strong></div><div><small>EVENTOS RECENTES</small><strong>{eventItems.length}</strong></div><div><small>STATUS</small><strong className="green">{cameras.isError || events.isError ? 'Atenção' : 'Online'}</strong></div></section><section><div className="section-title"><h2>Câmeras</h2><span>{cameraItems.filter((camera) => camera.enabled).length} ativas</span></div><div className="camera-grid">{cameraItems.map((camera) => <article className="camera" key={camera.id}><div className="camera-top"><span className="camera-dot"/><b>{camera.name}</b><span className="live">{camera.lastSeenAt ? 'ATIVA' : 'AGUARDANDO'}</span></div><p>Monitoramento de pessoas</p><small>{camera.lastSeenAt ? `Último sinal: ${new Date(camera.lastSeenAt).toLocaleString()}` : 'Sem sinal registrado'}</small></article>)}</div></section><section><div className="section-title"><h2>Eventos detectados</h2><span>{eventItems.length} registros</span></div><div className="events">{eventItems.length === 0 ? <div className="empty">Nenhum evento registrado ainda.</div> : eventItems.map((event) => <article className="event" key={event.id}><div><span className="event-type">LIMIAR DE PESSOAS</span><h3>{event.cameraName}</h3><p>{new Date(event.startedAt).toLocaleString()} · Pico de {event.maxPeopleCount} pessoa(s)</p></div><a href={`${apiUrl.replace('/api/v1', '')}${event.videoUrl}`} target="_blank" rel="noreferrer">Reproduzir gravação ↗</a></article>)}</div></section></main>;
}

createRoot(document.getElementById('root')!).render(<AppProviders><App /></AppProviders>);
