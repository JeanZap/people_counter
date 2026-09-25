# People Counter

Aplicação local para detectar pessoas em câmeras RTSP, registrar eventos e disponibilizar as gravações em uma interface web.

## Componentes

- `api/src/PeopleCounter.WebApi`: API ASP.NET Core 8, controllers versionados e configuração centralizada.
- `api/src/PeopleCounter.Domain`: entidades e regras do domínio.
- `api/src/PeopleCounter.Application`: contratos e abstrações dos casos de uso.
- `api/src/PeopleCounter.Infrastructure`: EF Core e PostgreSQL.
- `model/worker.py`: captura RTSP, YOLO/ByteTrack e gravação de eventos.
- `front`: dashboard React com TypeScript estrito, React Query, serviços e configuração centralizada.

## Executar com Docker

1. Copie `.env.example` para `.env` e ajuste, se necessário, `RTSP_URL` e a senha do banco.
2. Suba a infraestrutura com `docker compose up --build`.
3. O banco cria automaticamente a câmera padrão. O Ultralytics gerencia a obtenção do modelo durante a inicialização do worker.
4. Abra `http://localhost:3000` para o dashboard. A API possui Swagger em `http://localhost:8080/swagger` e health check em `http://localhost:8080/api/health`.

Em um banco vazio, a API cria automaticamente esta câmera:

```json
{
  "name": "Entrada",
  "connectionString": "rtsp://usuario:senha@camera:554/onvif1",
  "threshold": 1,
  "durationSeconds": 1,
  "resetDurationSeconds": 4,
  "frameWidth": 1024,
  "frameHeight": 640,
  "areaJson": "[[327,289],[350,499],[578,496],[530,292]]"
}
```

O worker usa a primeira câmera cadastrada se `CAMERA_ID` não for informado. O caminho do modelo pode ser alterado por `MODEL_PATH` ou pela propriedade `model_path` do `model/config.json`. A seed só é aplicada quando não existe nenhuma câmera, portanto configurações existentes não são sobrescritas.

## Desenvolvimento

```powershell
dotnet build api/PeopleCounter.slnx
cd front
npm install
npm run build
```

O worker usa `API_URL`, `RTSP_URL`, `MODEL_PATH` e `EVENT_ROOT`. Para CPU, o processamento pode exigir reduzir resolução ou FPS. Para múltiplas câmeras, execute uma instância do worker por câmera e defina `CAMERA_ID`.
