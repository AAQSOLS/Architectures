/**
 * File, Blob, and download utilities.
 */

/**
 * Formats a raw byte count into human-readable units (B, KB, MB, GB, TB).
 * Example: 1048576 -> "1 MB"
 */
export function formatBytes(bytes: number | null | undefined, decimals = 1): string {
  if (!bytes || bytes <= 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(decimals))} ${sizes[i]}`;
}

/**
 * Triggers an immediate browser file download from an in-memory Blob (e.g. CSV or PDF export).
 */
export function downloadBlob(blob: Blob, filename: string): void {
  const url = window.URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = filename;
  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);
  window.URL.revokeObjectURL(url);
}

/**
 * Triggers a download of arbitrary JavaScript objects as a JSON file.
 */
export function downloadJson(data: unknown, filename: string): void {
  const jsonStr = JSON.stringify(data, null, 2);
  const blob = new Blob([jsonStr], { type: 'application/json' });
  downloadBlob(blob, filename.endsWith('.json') ? filename : `${filename}.json`);
}

/**
 * Extracts the file extension in lowercase without the dot.
 * Example: "floorplan.PDF" -> "pdf"
 */
export function getFileExtension(filename: string | null | undefined): string {
  if (!filename) return '';
  const parts = filename.split('.');
  if (parts.length <= 1) return '';
  return parts[parts.length - 1]?.toLowerCase() ?? '';
}

/**
 * Converts a browser File instance into a Base64 string for instant local image previews.
 */
export function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => resolve(reader.result as string);
    reader.onerror = error => reject(error);
  });
}

/**
 * Checks whether a file is an accepted image format by MIME type.
 */
export function isImageFile(file: File): boolean {
  return file.type.startsWith('image/');
}
