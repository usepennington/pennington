---
title: Primeros Pasos
description: Primeros pasos con el ejemplo de DocSite localizado.
order: 30
---

# Primeros Pasos

Para añadir un nuevo idioma a tu propio sitio Pennington:

1. Abre `Program.cs` y llama a `loc.AddLocale(code, new LocaleInfo(displayName))`
   dentro de la acción `ConfigureLocalization` en `DocSiteOptions`.
2. Crea `Content/<code>/` y copia cada página que quieras traducir del
   árbol del idioma predeterminado, traduciendo el `title:` del front
   matter y el cuerpo.
3. Ejecuta `dotnet run` — `LanguageSwitcher` aparece en la cabecera del
   sitio tan pronto como `LocalizationOptions.Locales.Count > 1`.

No hay más cableado. El idioma predeterminado mantiene sus URLs sin cambios;
cada idioma adicional obtiene un prefijo de URL igual a su código.
