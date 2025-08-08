export const environment = {
  production: true,
  apiBaseUrl: 'https://your-production-api.com',
  auth: {
    issuer: 'SaaS.DocPaySystem',
    audience: 'SaaS.DocPaySystem.API',
    tokenKey: 'access_token',
    refreshTokenKey: 'refresh_token'
  },
  features: {
    enableNotifications: true,
    enableReports: true,
    enableAI: true,
    maxFileUploadSize: 10485760 // 10MB
  }
};
