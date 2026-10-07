import type { MetadataRoute } from "next";

export default function robots(): MetadataRoute.Robots {
  const baseUrl =
    process.env["NEXT_PUBLIC_APP_URL"] ?? "https://modularsaas.local";

  return {
    rules: {
      userAgent: "*",
      allow: "/",
      disallow: ["/api/", "/portal/"],
    },
    sitemap: `${baseUrl}/sitemap.xml`,
  };
}
