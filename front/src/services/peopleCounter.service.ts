import { BASE_CONFIG } from 'utils/configBase';
import { Camera, DetectionEvent } from 'domain/types';

async function get<T>(resource: string): Promise<T> {
	const response = await fetch(`${BASE_CONFIG.baseApiUrl}/${resource}`);
	if (!response.ok) throw new Error(`Falha ao consultar ${resource}`);
	return response.json() as Promise<T>;
}

export function usePeopleCounterService() {
	return {
		listarCameras: () => get<Camera[]>('cameras'),
		listarEventos: () => get<DetectionEvent[]>('events'),
	};
}
