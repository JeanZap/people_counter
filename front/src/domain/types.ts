export type Camera = {
	id: string;
	name: string;
	enabled: boolean;
	lastSeenAt: string | null;
};

export type DetectionEvent = {
	id: string;
	cameraName: string;
	status: string;
	startedAt: string;
	endedAt: string | null;
	maxPeopleCount: number;
	videoUrl: string;
};
