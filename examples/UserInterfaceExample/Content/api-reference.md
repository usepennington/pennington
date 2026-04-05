---
title: "CloudFlow API Reference"
description: "Complete API documentation for CloudFlow data processing platform"
tags:
  - api
  - reference
  - endpoints
order: 300
isDraft: false
---
Welcome to the CloudFlow API Reference! Here you'll find everything you need to integrate, automate, and extend the CloudFlow data processing platform. All endpoints, authentication methods, and error codes are documented below, with sample requests and responses.


## Authentication

All API requests must be authenticated. CloudFlow supports both API keys and OAuth 2.0 tokens. Unauthenticated requests will be rejected with a `401 Unauthorized` error.


### API Key Authentication

To use API key authentication, include your key in the `Authorization` header:

```http
GET /api/v1/pipelines
Authorization: ApiKey FAKE-API-KEY-123456
```


### OAuth 2.0 Flow

For user-specific access, use OAuth 2.0. Redirect users to `/auth/fake-oauth` and exchange the code for a token:

```http
POST /api/v1/oauth/token
Content-Type: application/json
{
  "code": "FAKE-OAUTH-CODE-7890"
}
```


### Rate Limiting

CloudFlow enforces rate limits to ensure fair usage. Exceeding limits returns:

```json
{
  "error": "Rate limit exceeded",
  "retry_after": 42
}
```


## Pipeline Management

Manage your data pipelines with these endpoints. Pipelines are the core units of data processing in CloudFlow.


### Create Pipeline

Create a new pipeline by POSTing a JSON definition:

```http
POST /api/v1/pipelines
Content-Type: application/json
Authorization: Bearer FAKE-TOKEN-ABCDEF

{
  "name": "MyFakePipeline",
  "steps": [
    { "type": "source", "config": { "path": "/fake/data.csv" } },
    { "type": "transform", "config": { "operation": "uppercase" } },
    { "type": "sink", "config": { "target": "db://fake-database" } }
  ]
}
```


### List Pipelines

Retrieve all pipelines:

```http
GET /api/v1/pipelines
Authorization: Bearer FAKE-TOKEN-ABCDEF
```


### Update Pipeline Configuration

Update a pipeline's configuration:

```http
PUT /api/v1/pipelines/{pipelineId}
Content-Type: application/json
Authorization: Bearer FAKE-TOKEN-ABCDEF
{
  "steps": [ { "type": "transform", "config": { "operation": "reverse" } } ]
}
```


### Delete Pipeline

Delete a pipeline:

```http
DELETE /api/v1/pipelines/{pipelineId}
Authorization: Bearer FAKE-TOKEN-ABCDEF
```


## Data Processing

Submit jobs, monitor status, and retrieve results for your data pipelines.


### Submit Processing Job

Start a new processing job:

```http
POST /api/v1/jobs
Content-Type: application/json
Authorization: Bearer FAKE-TOKEN-ABCDEF
{
  "pipelineId": "fake-pipeline-123",
  "input": "s3://fake-bucket/data.csv"
}
```


### Monitor Job Status

Check job status:

```http
GET /api/v1/jobs/{jobId}/status
Authorization: Bearer FAKE-TOKEN-ABCDEF
```


### Retrieve Job Results

Get job results:

```http
GET /api/v1/jobs/{jobId}/results
Authorization: Bearer FAKE-TOKEN-ABCDEF
```


## Data Sources

Configure and test connections to your data sources. Supported sources include fake databases, cloud buckets, and CSV files.


### Configure Data Source

Add a new data source:

```json
{
  "type": "database",
  "connectionString": "Server=fake-db;Database=fake;User Id=fake;Password=fake;"
}
```


### Test Connection

Test a data source connection:

```http
POST /api/v1/datasources/test
Content-Type: application/json
Authorization: Bearer FAKE-TOKEN-ABCDEF
{
  "connectionString": "Server=fake-db;Database=fake;User Id=fake;Password=fake;"
}
```


### Data Source Metadata

Retrieve metadata for a data source:

```http
GET /api/v1/datasources/{id}/metadata
Authorization: Bearer FAKE-TOKEN-ABCDEF
```


## Error Handling

All errors are returned in a consistent format. See below for examples and common codes.


### Error Response Format

Example error response:

```json
{
  "error": "Invalid pipeline configuration",
  "code": 4001,
  "details": "Step 'transform' missing required field 'operation'"
}
```


### Common Error Codes

- `4001`: Invalid pipeline configuration
- `4010`: Unauthorized
- `4040`: Resource not found
- `4290`: Rate limit exceeded
- `5000`: Internal server error
