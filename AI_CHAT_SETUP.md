# AutoEdge AI Chat Assistant Setup

## Overview
The AutoEdge AI Chat Assistant is a floating chat widget that provides instant support for customers and applicants. It uses the OpenRouter API with the free Mistral-7B model.

## Features
- **Customer Support**: Helps with car purchases, servicing, financing, and bookings
- **Recruitment Support**: Guides applicants through job applications, assessments, and interviews
- **Always Available**: Floating chat widget on every page
- **Smart Responses**: AI-powered responses using OpenRouter's free tier

## Setup Instructions

### 1. Get Your OpenRouter API Key
1. Visit [OpenRouter.ai](https://openrouter.ai)
2. Sign up for a free account
3. Navigate to your API keys section
4. Copy your API key

### 2. Configure appsettings.json
Update the AI configuration in `appsettings.json`:

```json
"AI": {
  "BaseUrl": "https://openrouter.ai/api/v1/chat/completions",
  "ApiKey": "YOUR_OPENROUTER_KEY_HERE",
  "Model": "mistralai/mistral-7b-instruct:free",
  "Temperature": 0.4,
  "MaxTokens": 1200
}
```

### 3. Files Created
The following files were added to the project:

- **Services/AIAssistantService.cs** - Main AI service implementation
- **Services/IAIAssistantService.cs** - Service interface
- **Controllers/ChatController.cs** - API controller for chat requests
- **Views/Shared/_Layout.cshtml** - Updated with chat widget (included at bottom)

### 4. Service Registration
The service is already registered in `Program.cs`:
```csharp
builder.Services.AddHttpClient<AutoEdge.Services.IAIAssistantService, AutoEdge.Services.AIAssistantService>();
```

## Usage

### For Users
1. Look for the floating chat icon (💬) in the bottom-right corner
2. Click to open the chat widget
3. Type your question about cars, services, or jobs
4. Press Enter or click Send
5. Receive AI-powered responses

### Example Questions
- "What cars do you have for sale?"
- "How do I book a service appointment?"
- "Tell me about the recruitment process"
- "What documents do I need for financing?"

## Customization

### Change the AI Model
In `appsettings.json`, change the Model value:
```json
"Model": "mistralai/mistral-7b-instruct:free"  // Free model
"Model": "anthropic/claude-3-haiku"           // Paid model
```

### Adjust Response Style
Modify the `Temperature` and `MaxTokens` values:
- **Temperature**: Controls creativity (0.0-1.0, lower = more focused)
- **MaxTokens**: Maximum response length (default: 1200)

### Update System Prompt
Edit the system message in `Services/AIAssistantService.cs`:
```csharp
var systemMessage = @"You are the AutoEdge AI Assistant...";
```

## Troubleshooting

### Chat widget not appearing
- Check if `appsettings.json` has the correct AI configuration
- Verify the API key is valid
- Check browser console for JavaScript errors

### "API Error" messages
- Verify your OpenRouter API key is correct
- Check your internet connection
- Ensure OpenRouter API is accessible

### Slow responses
- The free model may have rate limits
- Consider upgrading to a paid model for faster responses
- Check network latency

## Security Notes
- API keys are stored in `appsettings.json` (keep secure in production)
- Chat requests are sent to OpenRouter servers
- No chat history is stored locally (stateless)

## Cost Considerations
- **Free Model**: `mistralai/mistral-7b-instruct:free` - Unlimited use
- **Paid Models**: Varies by model, check OpenRouter pricing
- Monitor usage in OpenRouter dashboard

## Future Enhancements
- Chat history persistence
- Multi-language support
- Voice input/output
- Integration with booking system
- Live agent handoff
