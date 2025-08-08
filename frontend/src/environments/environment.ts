export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000',
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
