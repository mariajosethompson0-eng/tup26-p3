# Biblioteca de prompts para automatizar examenes

Este archivo contiene dos prompts reutilizables:

1. uno para generar preguntas multiple choice a partir de apuntes en Markdown
2. otro para generar una pagina web autocontenida que convierta ese archivo de preguntas en un examen interactivo

La idea es que puedas copiar, pegar y cambiar solo las variables entre `{{...}}`.

---

## Prompt 1: generar preguntas multiple choice

Usa este prompt cuando quieras transformar uno o varios apuntes en un archivo Markdown de preguntas de examen.

```text
Quiero que actues como generador de preguntas de examen multiple choice a partir de apuntes en Markdown.

Tarea:
- Lee los siguientes archivos fuente: {{archivos_fuente}}
- Genera un unico archivo Markdown llamado {{archivo_salida}}
- Las preguntas deben salir exclusivamente del contenido de esos apuntes. No inventes temas externos ni agregues conocimiento que no aparezca en el material.

Objetivo pedagogico:
- Crear preguntas de examen claras, cortas y utiles para alumnos.
- Mezclar preguntas conceptuales con preguntas de lectura de codigo cuando tenga sentido.
- Mantener dificultad media, con algunas preguntas mas directas y otras que exijan interpretar ejemplos.

Formato obligatorio del archivo de salida:
- Agrupa las preguntas por archivo fuente.
- Para todo el archivo usa un encabezado `#` con el titulo general del examen, por ejemplo `Examen de Terminal, Git y C#`.
- Para cada grupo usa un encabezado `##` con el titulo del tema, tomado del `#` principal del archivo fuente.
- Para cada pregunta usa un encabezado `###` breve que nombre el subtema.
- Debajo del `###` escribe la pregunta numerada en forma corrida: `1)`, `2)`, `3)`, etc.
- Si una pregunta necesita codigo, usa bloques fenced, por ejemplo:
  ```cs
  int x = 10;
  ```
- Las opciones deben escribirse exactamente asi:
  - [ ] opcion incorrecta
  - [x] opcion correcta
  - [ ] opcion incorrecta
- Debe haber exactamente 3 opciones por pregunta.
- Debe haber exactamente 1 respuesta correcta por pregunta.
- Entre preguntas usa un separador `---`.

Reglas obligatorias:
- Mezcla la posicion de la respuesta correcta. No la pongas siempre en la primera, segunda o tercera opcion.
- Evita patrones repetitivos en el orden de respuestas correctas.
- Las opciones incorrectas deben ser plausibles, no absurdas.
- No repitas la misma pregunta con redaccion distinta.
- No repitas literalmente largos fragmentos del apunte.
- Usa un espanol claro y simple.
- Respeta tildes y signos de pregunta cuando correspondan.
- Si el material incluye Bash, Git o C#, mantene el formateo adecuado de comandos, tipos y bloques de codigo.

Cantidad:
- Genera aproximadamente {{cantidad_total}} preguntas en total.
- Reparti las preguntas de forma razonable entre los archivos fuente.

Validaciones antes de terminar:
- Verifica que todas las preguntas esten numeradas de corrido.
- Verifica que cada pregunta tenga 3 opciones.
- Verifica que cada pregunta tenga 1 sola opcion marcada con `[x]`.
- Verifica que los grupos `##` correspondan a los titulos reales de los apuntes.
- Verifica que las respuestas correctas queden distribuidas en posiciones variadas.

Modo de entrega:
- Si estas trabajando dentro de un repo o workspace editable, crea o actualiza directamente el archivo `{{archivo_salida}}`.
- Si no podes editar archivos, devuelve solamente el contenido completo del Markdown final, sin explicaciones extra.
```

---

## Prompt 2: generar la pagina web del examen

Usa este prompt cuando ya tengas un archivo Markdown de preguntas en el formato anterior y quieras convertirlo en una app web autocontenida.

```text
Quiero que actues como desarrollador frontend y construyas una app web autocontenida que convierta un archivo Markdown de preguntas multiple choice en una pagina de examen interactiva.

Entrada principal:
- Archivo de preguntas: {{archivo_preguntas}}

Salida esperada:
- Un unico archivo HTML llamado {{archivo_html}}
- Debe ser completamente autocontenido: todo el CSS y todo el JavaScript deben quedar embebidos en el mismo archivo.
- No uses frameworks ni dependencias externas.

Formato del archivo de preguntas a soportar:
- Los temas estan agrupados con `##`
- Cada pregunta empieza con un `###`
- Debajo aparece el enunciado numerado, por ejemplo `1) ...`
- Puede haber bloques de codigo fenced, por ejemplo ` ```cs ` o ` ```bash `
- Las opciones vienen en este formato:
  - [ ] opcion incorrecta
  - [x] opcion correcta
  - [ ] opcion incorrecta

Comportamiento obligatorio de la app:
- Debe cargar automaticamente `{{archivo_preguntas}}` si el archivo esta en la misma carpeta.
- Debe incluir tambien un selector de archivo como fallback por si no se puede cargar automaticamente.
- Debe renderizar la pagina con estetica de examen multiple choice en papel.
- Debe mostrar el contenido Markdown con presentacion rica:
  - encabezados
  - parrafos
  - inline code
  - bloques de codigo
- Debe incluir coloreo de sintaxis para bloques de codigo, al menos para `cs`/`csharp` y `bash`/`sh`/`zsh`.
- En lugar de mostrar `- [ ]` o `- [x]`, debe renderizar checkboxes vacios.
- Visualmente deben ser checkboxes, pero funcionalmente solo se debe poder elegir una opcion por pregunta.
- Ninguna opcion debe aparecer seleccionada al cargar el examen.
- Debe haber un boton `Finalizar`.
- El boton `Finalizar` solo debe activarse cuando todas las preguntas tengan una respuesta elegida.
- Al presionar `Finalizar`, la pagina debe calcular el puntaje final usando las respuestas correctas marcadas en el Markdown.
- Debe mostrar:
  - porcentaje de respuestas correctas
  - cantidad de respuestas correctas sobre el total
- Si lo consideras util, tambien podes marcar visualmente preguntas correctas e incorrectas despues de finalizar.

Requisitos de UX:
- El diseño debe sentirse como una hoja de examen cuidada, no como un formulario generico.
- Debe funcionar bien en desktop y mobile.
- La lectura del codigo debe ser clara.
- El examen debe ser comodo de responder incluso cuando haya muchas preguntas.

Requisitos tecnicos:
- No uses librerias externas para parsear Markdown.
- Implementa un parser simple en JavaScript para el formato concreto de este examen.
- No dependas de internet.
- Todo debe funcionar abriendo el HTML localmente, salvo la carga automatica del archivo por defecto, que puede fallar por restricciones del navegador. En ese caso el selector de archivo debe resolverlo.

Validaciones antes de terminar:
- Verifica que el parser agrupe correctamente por `##` y `###`.
- Verifica que cada pregunta acepte una sola respuesta.
- Verifica que `Finalizar` quede deshabilitado hasta completar todas las preguntas.
- Verifica que el porcentaje final se calcule correctamente.
- Verifica que el HTML final sea autocontenido.

Modo de entrega:
- Si estas trabajando dentro de un repo o workspace editable, crea o actualiza directamente el archivo `{{archivo_html}}`.
- Si no podes editar archivos, devuelve solamente el contenido completo del HTML final, sin explicaciones extra.
```

---

## Variables que vas a cambiar

Estas son las variables tipicas que vas a editar en los prompts:

- `{{archivos_fuente}}`
- `{{archivo_salida}}`
- `{{cantidad_total}}`
- `{{archivo_preguntas}}`
- `{{archivo_html}}`

---

## Ejemplo real para este repo

### Ejemplo del prompt de preguntas

```text
{{archivos_fuente}} = 02-terminal-y-bash-para-desarrollo.md, 03-git-y-github-control-de-versiones-y-colaboracion.md, 04-variables-memoria-y-tipos-basicos-en-csharp.md, 05-tipos-de-datos-y-conversiones-en-csharp.md
{{archivo_salida}} = preguntas-multiple-choice-02-al-05.md
{{cantidad_total}} = 20
```

### Ejemplo del prompt de pagina

```text
{{archivo_preguntas}} = preguntas-multiple-choice-02-al-05.md
{{archivo_html}} = examen-multiple-choice.html
```

---

## Recomendacion practica

Si queres automatizarlo todavia mejor, podes guardar:

- un prompt base para preguntas
- un prompt base para pagina web
- una variante "sin respuestas marcadas"
- una variante "con clave docente"

Asi convertis este flujo en una pequeña biblioteca reutilizable para nuevas unidades o nuevas materias.
