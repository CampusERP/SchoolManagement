import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import path from "path";
export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), "");
    const backendUrl = env.VITE_API_URL;
    return {
        plugins: [react(), tailwindcss()],
        resolve: {
            alias: {
                "@": path.resolve(__dirname, "./src"),
            },
        },
        build: {
            sourcemap: true,
            rollupOptions: {
                output: {
                    manualChunks(id) {
                        const normalizedId = id.replaceAll("\\", "/");
                        if (normalizedId.includes("/node_modules/react-router/") ||
                            normalizedId.includes("/node_modules/react-router-dom/")) {
                            return "router-vendor";
                        }
                        if (normalizedId.includes("/node_modules/@radix-ui/") ||
                            normalizedId.includes("/node_modules/@floating-ui/")) {
                            return "ui-vendor";
                        }
                    },
                },
            },
        },
        server: {
            port: 3000,
            proxy: {
                "/api": {
                    target: backendUrl,
                    changeOrigin: true,
                    secure: true,
                },
            },
        },
    };
});
