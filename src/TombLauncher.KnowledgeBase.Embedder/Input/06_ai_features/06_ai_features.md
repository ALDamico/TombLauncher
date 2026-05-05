# AI Features
**by Tomb Launcher Developers**  
*Last Edited: 05 May 2026*

## Does Tomb Launcher manage AI Models internally?
No, Tomb Launcher relies on the availability of an OpenAI-compatible or an LM Studio API in order to use LLM models.

## Can I choose a different embedding model?
Since the knowledge base embeddings are generated with nomic-text-embed, using that same embedding model inside Tomb 
Launcher is required and cannot be changed by the user.

## Are AI features mandatory?
No, AI features are opt-in. They are disabled by default.

## What models are best suited for Tomb Launcher?
The AI feature in Tomb Launcher do not require particularly large models. Here are a few that, in our tests, performed particularly well:
* Ministral 3 3B - tends to hallucinate sometimes, but provides okay responses, is fast, and doesn't require particularly large amounts of VRAM
* GPT OSS 20B - works okay, but we've experienced some hallucinations and at times doesn't stay in character
* Phi 4 - pretty good overall
* Qwen 3 Coder 30B - of all the models we've tested, this behaved the best, but is a reasoning model and requires larger amounts of VRAM.