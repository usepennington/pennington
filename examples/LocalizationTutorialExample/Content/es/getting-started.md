---
title: "Primeros Pasos"
description: "Aprenda cómo comenzar"
order: 10
---

## Instalación

Siga estos pasos para comenzar:

1. Instale el paquete
2. Configure su proyecto
3. Ejecute la aplicación

## Ejemplo Rápido

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMyService();
var app = builder.Build();
app.Run();
```
