# **?? Scalar API Documentation with JWT Authentication**

## **? Problem Solved!**

I've configured your SportPicks API to show a **proper authentication interface** in Scalar, just like Swagger! You now have an "Authorize" button that lets you set your bearer token once and use it for all requests.

## **?? What's New**

### **1. Authentication Button in Scalar UI**
- ? **"Authorize" button** in the top-right corner
- ? **Bearer token input field** 
- ? **Token persistence** across all API calls
- ? **Visual authentication indicators**

### **2. Enhanced API Documentation**
- ? **Organized by tags** (Authentication, NFL Data Synchronization, etc.)
- ? **Detailed endpoint descriptions** with examples
- ? **Response code documentation** (200, 401, 403, 500)
- ? **Usage examples** with curl commands

### **3. Improved Developer Experience**
- ? **Purple theme** for better readability
- ? **Request/response examples**
- ? **Authentication status indicators**

## **?? How to Use the Enhanced Scalar UI**

### **Step 1: Start Your Application**
```bash
dotnet run --project .\SportPicks.API
```

### **Step 2: Open Scalar Documentation**
Navigate to: `https://localhost:5001/scalar/v1`

### **Step 3: Authenticate**

1. **Click the "Authorize" button** (?? icon) in the top-right
2. **Enter your token** in one of these formats:
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
   or just:
   ```
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
3. **Click "Apply"** - the token is now set for all requests!

### **Step 4: Get Your Token**

**Option A: Use the Login Endpoint in Scalar**
1. Go to **Authentication > POST /api/v1/auth/login**
2. Use the "Try it out" button
3. Enter credentials:
   ```json
   {
     "emailOrUsername": "admin@example.com",
     "password": "your-password"
   }
   ```
4. **Copy the token** from the response
5. **Click "Authorize"** and paste the token

**Option B: Use curl/Postman**
```bash
curl -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"emailOrUsername":"admin@example.com","password":"your-password"}'
```

## **?? Key Features**

### **Authentication Status**
- **?? Locked icon**: Authentication required
- **?? Unlocked icon**: Authentication set and ready
- **Green checkmark**: Request will include your token

### **Organized Endpoints**
```
?? Authentication
  ??? POST /api/v1/auth/login
  ??? POST /api/v1/auth/logout  
  ??? POST /api/v1/auth/refresh

?? NFL Data Synchronization  
  ??? POST /api/v1/admin/nfl-sync/teams
  ??? POST /api/v1/admin/nfl-sync/matches
  ??? POST /api/v1/admin/nfl-sync/full ?? High Impact
  ??? POST /api/v1/admin/nfl-sync/matches/season/{season}

?? User Management
  ??? POST /api/v1/users
  ??? PUT /api/v1/users/{email}/password
```

### **Visual Indicators**
- **?? High Impact Operations** clearly marked
- **?? Authorization Requirements** visible per endpoint  
- **?? Response Examples** for each status code
- **? Usage Examples** with curl commands

## **?? Example Workflow**

### **1. First Time Setup**
```bash
# 1. Get your JWT token
POST /api/v1/auth/login
{
  "emailOrUsername": "admin@example.com", 
  "password": "SecurePassword123!"
}

# 2. Copy the "token" from response
# 3. Click "Authorize" in Scalar
# 4. Paste: Bearer eyJhbGciOiJIUzI1NiIs...
# 5. Click "Apply"
```

### **2. Using Protected Endpoints**
Now all your requests will **automatically include** the authorization header:

```bash
# This will work without manually adding auth headers
POST /api/v1/admin/nfl-sync/full
# Scalar automatically adds: Authorization: Bearer your-token
```

### **3. Token Management**
- **Automatic expiration handling**: Scalar shows when tokens are invalid
- **Easy token refresh**: Get new token and update via "Authorize" button
- **Persistent across sessions**: Token stays set until you clear it

## **?? Advanced Configuration**

Your API now includes:

### **OpenAPI Security Scheme**
```json
{
  "securitySchemes": {
    "Bearer": {
      "type": "http",
      "scheme": "bearer", 
      "bearerFormat": "JWT",
      "description": "Enter 'Bearer' followed by your JWT token"
    }
  }
}
```

### **Global Security Requirements**
All protected endpoints automatically show ?? and require authentication.

### **Comprehensive Documentation**
Every endpoint includes:
- **Detailed descriptions**
- **Parameter examples** 
- **Response samples**
- **Error code explanations**
- **Usage examples**

## **?? Benefits Over Manual Headers**

| Manual Headers | Scalar with Auth |
|----------------|------------------|
| ? Add header each request | ? Set once, use everywhere |
| ? Copy/paste token repeatedly | ? Persistent across requests |
| ? Easy to make typos | ? Validated token format |
| ? No expiration warnings | ? Clear auth status indicators |
| ? Manual token management | ? Easy token refresh workflow |

## **?? Ready to Use!**

Your Scalar API documentation now provides a **professional, Swagger-like experience** with:
- ? **One-click authentication**
- ? **Persistent bearer tokens** 
- ? **Visual auth indicators**
- ? **Comprehensive documentation**
- ? **Professional appearance**

No more manually copying authorization headers! ??