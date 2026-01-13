# Stage 1: Build the React application
FROM node:20-alpine AS build
WORKDIR /app

# Copy package.json and package-lock.json
# Using wildcards to ensure both are copied if available
COPY package*.json ./

# Install dependencies using npm ci for faster, reliable builds
# We use --frozen-lockfile equivalent behavior with npm ci
RUN npm ci

# Copy the rest of the application source code
# (Now faster thanks to .dockerignore)
COPY . .

# Build the application
RUN npm run build

# Stage 2: Serve the application using NGINX
# Using stable-alpine for a balance of stability and size
FROM nginx:stable-alpine

# Remove default nginx static assets
RUN rm -rf /usr/share/nginx/html/*

# Copy the custom NGINX configuration file
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy only the necessary build artifacts from the 'build' stage
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Start NGINX
ENTRYPOINT ["nginx", "-g", "daemon off;"]