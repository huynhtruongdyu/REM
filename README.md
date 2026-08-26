# REM — Resume Builder

**REM** is a free, client-side resume builder built with **Blazor WebAssembly**. It lets you create a clean, ATS‑friendly resume with a live split‑view editor, drag‑and‑drop section/entry reordering, multiple visual themes, undo/redo, and one‑click PDF export — all running in the browser with no backend.

Everything is stored locally in your browser (LocalStorage); you can also import/export the resume as a JSON file.

---

## Features

- **Split view** — editor on the left, live preview on the right.
- **Drag & drop** — reorder whole sections and individual entries (powered by SortableJS).
- **Live preview** — changes appear instantly; the preview is single‑column and **ATS‑friendly** (no tables/columns that trip up applicant‑tracking systems).
- **Themes** — switch between **Default**, **Minimalism**, and **Modern** (saved with the resume).
- **Undo / Redo** — full history (`Ctrl+Z` / `Ctrl+Y` or `Ctrl+Shift+Z`), including add/remove/reorder, import, clear, and sample load.
- **Auto‑save** — the resume is continuously saved to LocalStorage; a manual **Save** button is also available.
- **Import / Export** — load or download the resume as a JSON file.
- **Sample** — load a fully populated example resume.
- **Clear** — reset to a blank resume (with confirmation).
- **PDF** — **Preview** opens the generated PDF in a new tab; **Download PDF** saves it directly (generated client‑side via html2pdf.js, so no browser print headers/footers).

---

## Tech Stack

| Area        | Choice |
|-------------|--------|
| Framework   | .NET 10 Blazor WebAssembly |
| UI / CSS    | Bootstrap 5 |
| Drag & drop | SortableJS (via JSInterop) |
| PDF export  | html2pdf.js |
| Storage     | Browser LocalStorage (JSInterop) |
| Deploy      | GitHub Pages (static site) |

Libraries are managed with **libman** and restored locally to `wwwroot/lib`.

---

## Getting Started

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
# restore client libraries (SortableJS, html2pdf.js, jQuery)
dotnet libman restore

# run the app (default on https://localhost:5114 or http://localhost:5114)
dotnet run
```

Open the root URL — the resume builder loads as the home page. The first load seeds a **sample resume** so you can explore the UI immediately.

---

## Usage

The toolbar (top of the page) groups the commands:

| Group | Buttons | Purpose |
|-------|---------|---------|
| History | **Undo**, **Redo** | Step through edit history (also `Ctrl+Z` / `Ctrl+Y`). |
| Theme | **Theme** dropdown | Switch Default / Minimalism / Modern. |
| File | **Import**, **Export** | Load or download a `.json` resume. |
| Data | **Save**, **Sample**, **Clear** | Persist to LocalStorage, load example, reset (with confirm). |
| PDF | **Preview**, **Download PDF** | Open PDF in a new tab, or save it directly. |

### Editing
- Type in any field; the preview updates live.
- Drag the handle on a **section header** to reorder sections.
- Drag the handle on an **item** (experience, skill, etc.) to reorder entries within a section.
- Add/remove items with the buttons inside each section card.

### Tips
- The **Preview/Download PDF** output intentionally has **no** document title, URL, or page/date footers (it is rendered client‑side, not via the browser print pipeline).
- The resume JSON (and selected theme) is what gets saved/exported, so you can move it between browsers/devices.

---

## Project Structure

```
REM/
├── Models/                 # Resume data models + SampleResume + SectionDefinitions
├── States/                 # ResumeState (state, undo/redo, edit context)
├── Services/               # StorageService (LocalStorage + JSON import/export)
├── Components/
│   ├── Editor/             # Editor + per-section editors + SortableList
│   └── Preview/            # ATS-friendly, themeable resume preview
├── Pages/
│   ├── Home.razor          # The builder (root page)
│   └── NotFound.razor      # Redirects unknown routes to home
├── Layout/                 # BuilderLayout (full-height split view)
├── wwwroot/
│   ├── js/app.js           # SortableJS + shortcuts + PDF interop
│   ├── lib/                # Restored client libraries (libman)
│   └── css/app.css         # Editor, preview themes, and print styles
├── libman.json
└── .github/workflows/deploy.yml   # GitHub Pages deployment
```

---

## Deployment (GitHub Pages)

A GitHub Actions workflow (`.github/workflows/deploy.yml`) builds and deploys the static site to GitHub Pages on every push to `main`.

1. In the repo **Settings → Pages → Build and deployment → Source**, choose **GitHub Actions**.
2. Push to `main` (or trigger the workflow manually).

The workflow keeps the `<base href>` as `/` so the site is served from the domain root. A `wwwroot/CNAME` file sets the custom domain (currently `rem.duyhuynh.net`), so the site is published at `https://rem.duyhuynh.net/`. Note: because the base is `/`, the project‑page URL `https://<user>.github.io/<repo>/` is no longer used for this deployment.

> Note: `BlazorEnableCompression` is disabled in the project so the precompressed `.br`/`.gz` assets (which GitHub Pages does not serve with the correct `Content-Encoding`) don't break loading.

---

## License

This project is provided as‑is for personal and educational use.
