/**
 * Triggers a client-side file download from a Blob or URL.
 */
export function downloadFile(blobOrUrl: Blob | string, filename: string): void {
  if (typeof window === "undefined") return;

  const url =
    typeof blobOrUrl === "string" ? blobOrUrl : URL.createObjectURL(blobOrUrl);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = filename;
  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);

  if (typeof blobOrUrl !== "string") {
    URL.revokeObjectURL(url);
  }
}
