import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode } from 'react';

const queryClient = new QueryClient({
	defaultOptions: { queries: { retry: 2, refetchOnWindowFocus: true } },
});

export function AppProviders({ children }: { children: ReactNode }) {
	return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}
