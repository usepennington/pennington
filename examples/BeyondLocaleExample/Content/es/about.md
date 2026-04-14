---
title: Acerca de
description: Acerca de este ejemplo de DocSite localizado.
order: 20
---

# Acerca de

Este es un DocSite mínimo que demuestra **URLs conscientes del idioma**.
Cada archivo markdown bajo `Content/` es la versión en inglés (el idioma
predeterminado). Cada archivo correspondiente bajo `Content/es/` es la
traducción al español.

Cuando un visitante navega a `/es/about`, el middleware
`LocaleDetectionMiddleware` elimina el prefijo `/es`, guarda `"es"` en
`LocaleContext`, y el `ContentResolver` del DocSite busca el markdown
en `Content/es/about.md`. Si falta un archivo en español, el resolvedor
recurre a la copia en inglés y marca la página como una traducción de
reserva para que el lector lo sepa.
