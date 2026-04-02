# AstraLab

## What is AstraLab?

AstraLab is an end-to-end data intelligence platform for turning raw datasets into stakeholder-ready outputs. It brings ingestion, profiling, exploration, transformation, AI-assisted analysis, machine learning experimentation, and reporting into one cohesive workspace so teams can move from raw files to clear decisions without switching tools.

## Why Choose AstraLab?

Unified workflow: AstraLab keeps dataset ingestion, profiling, AI assistance, ML experimentation, and reporting in one platform so analysis work does not feel fragmented.

Practical intelligence: The platform combines system-generated analytics with AI-assisted summaries to help users understand data quality, transformations, experiment outcomes, and key risks faster.

Stakeholder-ready outputs: AstraLab supports dashboards, stored insights, generated reports, and downloadable exports so technical work can be turned into readable business-facing deliverables.

Version-aware analysis: Dataset versions, transformation history, and experiment context are linked together so users can understand how results changed over time.

# Documentation

- [Frontend README](./frontend/README.md)
- [Backend README](./aspnet-core/README.md)
- [ML Executor README](./ml-executor/README.md)
- [Backend Structure Guide](./aspnet-core/BACKEND_STRUCTURE.md)

## Software Requirement Specification

### Overview

AstraLab is designed to help teams ingest datasets, understand their quality, refine them through transformations, generate AI-assisted insights, run machine learning experiments, and present results through dashboards, reports, and exports. The product focuses on making advanced analytical workflows easier to manage, easier to explain, and easier to share.

### Components and Functional Requirements

**1. Authentication and tenant-aware access**

- Users can register and sign in to AstraLab.
- Authenticated users can access protected workspaces across the platform.
- Data access is scoped so users only interact with datasets, reports, and outputs they are authorized to see.

**2. Dataset ingestion and versioning**

- Users can upload supported dataset files into the platform.
- Each upload creates or extends dataset history through version-aware records.
- Users can revisit dataset details and work with version-specific analytical context.

**3. Profiling and schema insights**

- Users can view dataset profiling outputs such as row counts, health metrics, and schema details.
- The system can surface quality indicators, anomalies, duplicates, and column-level concerns.
- Profiling results feed downstream dashboards, AI summaries, and reports.

**4. Dataset exploration**

- Users can open a dataset exploration view to inspect structure and content contextually.
- Exploration is connected to dataset versions so users understand which version they are reviewing.
- Exploration supports deeper follow-up work in profiling, transformations, AI, and ML.

**5. Transformation pipeline management**

- Users can define and run dataset transformation steps.
- Transformation outcomes are tracked and connected to the broader dataset workflow.
- Users can review transformation history as part of dataset understanding and reporting.

**6. AI assistant and stored interaction history**

- Users can ask dataset-specific and workspace-level AI questions.
- AI interactions can incorporate dataset, profile, and experiment context.
- The platform stores AI responses and analytics insights for later retrieval and reuse.

**7. ML experiment workspace and experiment-aware AI insights**

- Users can configure and run ML experiments for supported datasets.
- Experiment results include metrics, feature importance, artifacts, and related insight generation.
- ML outputs feed the unified analytics layer and stakeholder-facing summaries.

**8. Analytics dashboard, reports, and exports**

- Users can view an executive-style analytics dashboard for a selected dataset version.
- Users can generate stakeholder reports and export analytical outputs in CSV and PDF formats.
- Stored reports and exports remain available for later review and download through the platform.

### Architecture Reference

The backend follows a layered modular-monolith structure documented in the [Backend Structure Guide](./aspnet-core/BACKEND_STRUCTURE.md). This guide is the source of truth for backend capability placement, project responsibilities, and architectural boundaries.

# Design

## [Wireframes](https://www.figma.com/design/PTReckDJrJfzReEqFha98j/AstraLab?node-id=0-1&p=f&t=w1kfo4GUS0RLeMmV-0)

## [Domain Model](https://drive.google.com/file/d/1JgfLpc7gADfOlFkCtq6uUPhXahCiY-75/view?usp=sharing)

# Running the Application

## Frontend

See the [frontend README](./frontend/README.md) for environment variables, development commands, and production build instructions.

## Backend

See the [backend README](./aspnet-core/README.md) for solution layout, configuration areas, host startup, and migration steps.

## ML Executor

See the [ML executor README](./ml-executor/README.md) for Python setup, required environment variables, and local executor startup instructions.

## Recommended Local Startup Order

1. Configure and run the backend migrator and API host.
2. Start the ML executor service.
3. Start the frontend development server.

This keeps authentication, dataset workflows, ML execution callbacks, and reporting features available during local development.
