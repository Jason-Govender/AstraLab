# AstraLab Frontend

## Overview

The AstraLab frontend is the user-facing workspace for dataset ingestion, profiling, exploration, AI assistance, ML experimentation, analytics dashboards, and stakeholder reporting. It is built as a Next.js App Router application and serves as the main interaction layer for the platform.

## Stack

- Next.js App Router
- React
- TypeScript
- Ant Design
- `antd-style`

## Key Routes and Workspaces

- `/login` and `/register` for authentication
- `/dashboard` for the executive analytics summary
- `/datasets` for dataset management
- `/datasets/upload` for dataset ingestion
- `/datasets/[datasetId]` for dataset details and profiling context
- `/datasets/[datasetId]/explore` for dataset exploration
- `/datasets/[datasetId]/transform` for transformation workflows
- `/datasets/[datasetId]/assistant` and `/ai-assistant` for AI-assisted analysis
- `/ml-workspace` for experiment management and ML insights
- `/reports` for report generation, export actions, and stored stakeholder outputs

## Environment Variables

Create a local `.env` file based on `.env.example`.

Required variables:

- `NEXT_PUBLIC_API_BASE_URL`
  - Public backend base URL used by the browser-facing client.
- `BACKEND_INTERNAL_URL`
  - Internal backend base URL used for server-side requests when needed.

Example values are provided in [`./.env.example`](./.env.example).

## Development

Install dependencies:

```bash
npm install
```

Start the development server:

```bash
npm run dev
```

The default local frontend URL is typically [http://localhost:3000](http://localhost:3000).

## Production

Build the application:

```bash
npm run build
```

Start the production server:

```bash
npm start
```

## Architecture Notes

The frontend uses App Router route groups under `src/app` to separate authentication routes from protected workspace routes. Shared UI is organized under `src/components`, shared domain and API contracts live under `src/types` and `src/services`, and route-scoped state is managed with provider-driven patterns under `src/providers`.

This keeps page files thin while allowing dashboard, reports, AI, dataset, and ML workspaces to manage their own focused state and actions cleanly.

## Related Documentation

- [Root README](../README.md)
- [Backend README](../aspnet-core/README.md)
- [ML Executor README](../ml-executor/README.md)
