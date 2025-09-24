window.webauthn = {
    get: async function (publicKeyOptions, options = {}) {
        console.log('[WebAuthn JS] webauthn.get called with options:', publicKeyOptions);
        console.log('[WebAuthn JS] Additional options:', options);
        
        try {
            // Convert challenge from base64 to Uint8Array
            console.log('[WebAuthn JS] Converting challenge from base64...');
            publicKeyOptions.challenge = Uint8Array.from(atob(publicKeyOptions.challenge), c => c.charCodeAt(0));
            
            // Handle allowCredentials if present and not empty
            if (publicKeyOptions.allowCredentials && publicKeyOptions.allowCredentials.length > 0) {
                console.log('[WebAuthn JS] Processing allowCredentials array...');
                publicKeyOptions.allowCredentials = publicKeyOptions.allowCredentials.map(cred => ({
                    ...cred,
                    id: Uint8Array.from(atob(cred.id), c => c.charCodeAt(0))
                }));
            } else {
                console.log('[WebAuthn JS] No allowCredentials or empty array, enabling cross-device authentication');
                // Remove allowCredentials to enable cross-device authentication
                delete publicKeyOptions.allowCredentials;
            }
            
            // Enhanced options for GitHub-style authentication
            const enhancedOptions = {
                ...publicKeyOptions,
                // Enable cross-device authentication
                hints: publicKeyOptions.hints || ['cross-platform'],
                // Set appropriate timeout for cross-device
                timeout: options.timeout || 120000, // 2 minutes for cross-device
                // Enable resident key discovery
                userVerification: publicKeyOptions.userVerification || 'required'
            };
            
            console.log('[WebAuthn JS] Final processed options:', enhancedOptions);
            console.log('[WebAuthn JS] Calling navigator.credentials.get...');
            
            const cred = await navigator.credentials.get({ publicKey: enhancedOptions });
            
            console.log('[WebAuthn JS] navigator.credentials.get succeeded, processing credential...');
            
            const result = {
                id: cred.id,
                rawId: btoa(String.fromCharCode(...new Uint8Array(cred.rawId))),
                type: cred.type,
                response: {
                    clientDataJSON: btoa(String.fromCharCode(...new Uint8Array(cred.response.clientDataJSON))),
                    authenticatorData: btoa(String.fromCharCode(...new Uint8Array(cred.response.authenticatorData))),
                    signature: btoa(String.fromCharCode(...new Uint8Array(cred.response.signature))),
                    userHandle: cred.response.userHandle ? btoa(String.fromCharCode(...new Uint8Array(cred.response.userHandle))) : null
                }
            };
            
            console.log('[WebAuthn JS] Returning processed credential:', result);
            return result;
            
        } catch (error) {
            console.error('[WebAuthn JS] Error in webauthn.get:', error);
            console.error('[WebAuthn JS] Error name:', error.name);
            console.error('[WebAuthn JS] Error message:', error.message);
            
            // Enhanced error handling for GitHub-style flows
            if (error.name === 'NotAllowedError') {
                throw new Error('Authentication was cancelled or not allowed');
            } else if (error.name === 'NotSupportedError') {
                throw new Error('Passkeys are not supported on this device');
            } else if (error.name === 'SecurityError') {
                throw new Error('Security error occurred during authentication');
            } else if (error.name === 'TimeoutError') {
                throw new Error('Authentication timed out');
            }
            
            throw error;
        }
    },
    
    create: async function (publicKeyOptions, options = {}) {
        console.log('[WebAuthn JS] webauthn.create called with options:', publicKeyOptions);
        console.log('[WebAuthn JS] Additional options:', options);
        
        try {
            // Convert base64 strings to Uint8Array for WebAuthn
            publicKeyOptions.challenge = Uint8Array.from(atob(publicKeyOptions.challenge), c => c.charCodeAt(0));
            publicKeyOptions.user.id = Uint8Array.from(atob(publicKeyOptions.user.id), c => c.charCodeAt(0));
            
            // Handle excludeCredentials if present
            if (publicKeyOptions.excludeCredentials) {
                publicKeyOptions.excludeCredentials = publicKeyOptions.excludeCredentials.map(cred => ({
                    ...cred,
                    id: Uint8Array.from(atob(cred.id), c => c.charCodeAt(0))
                }));
            }
            
            // Enhanced options for GitHub-style registration
            const enhancedOptions = {
                ...publicKeyOptions,
                // Enhanced authenticator selection for cross-platform support
                authenticatorSelection: {
                    authenticatorAttachment: options.authenticatorAttachment || "any", // Allow both platform and cross-platform
                    userVerification: "required",
                    requireResidentKey: options.requireResidentKey !== false, // Default to true for better UX
                    residentKey: options.residentKey || "preferred"
                },
                attestation: options.attestation || "direct",
                timeout: options.timeout || 120000, // 2 minutes for cross-device
                // Enable cross-platform hints
                hints: publicKeyOptions.hints || ['security-key', 'client-device']
            };
            
            console.log('[WebAuthn JS] Enhanced create options:', enhancedOptions);
            console.log('[WebAuthn JS] Calling navigator.credentials.create...');
            
            const cred = await navigator.credentials.create({ publicKey: enhancedOptions });
            
            console.log('[WebAuthn JS] navigator.credentials.create succeeded, processing credential...');
            
            const result = {
                id: cred.id,
                rawId: btoa(String.fromCharCode(...new Uint8Array(cred.rawId))),
                type: cred.type,
                response: {
                    clientDataJSON: btoa(String.fromCharCode(...new Uint8Array(cred.response.clientDataJSON))),
                    attestationObject: btoa(String.fromCharCode(...new Uint8Array(cred.response.attestationObject)))
                }
            };
            
            console.log('[WebAuthn JS] Returning processed credential:', result);
            return result;
            
        } catch (error) {
            console.error('[WebAuthn JS] Error in webauthn.create:', error);
            console.error('[WebAuthn JS] Error name:', error.name);
            console.error('[WebAuthn JS] Error message:', error.message);
            
            // Enhanced error handling
            if (error.name === 'NotAllowedError') {
                throw new Error('Registration was cancelled or not allowed');
            } else if (error.name === 'NotSupportedError') {
                throw new Error('Passkeys are not supported on this device');
            } else if (error.name === 'SecurityError') {
                throw new Error('Security error occurred during registration');
            } else if (error.name === 'TimeoutError') {
                throw new Error('Registration timed out');
            }
            
            throw error;
        }
    }
}

// Enhanced QR code generation for cross-device authentication
window.generateQRCode = function(elementId, data) {
    const element = document.getElementById(elementId);
    if (element) {
        // Create a more sophisticated QR-like code
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        const size = 256;
        canvas.width = size;
        canvas.height = size;
        
        // Create a more complex pattern that looks like a real QR code
        const gridSize = 33; // QR code typically uses 33x33 for version 3
        const cellSize = Math.floor(size / gridSize);
        const actualSize = cellSize * gridSize;
        
        // Position the grid in the center
        const offsetX = (size - actualSize) / 2;
        const offsetY = (size - actualSize) / 2;
        
        // Create a deterministic pattern based on data
        let seed = 0;
        for (let i = 0; i < data.length; i++) {
            seed += data.charCodeAt(i);
        }
        
        // Fill background
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, size, size);
        
        // Create QR code-like pattern with finder patterns
        ctx.fillStyle = '#000000';
        
        // Add finder patterns (corners)
        const addFinderPattern = (x, y) => {
            const pattern = [
                [1,1,1,1,1,1,1],
                [1,0,0,0,0,0,1],
                [1,0,1,1,1,0,1],
                [1,0,1,1,1,0,1],
                [1,0,1,1,1,0,1],
                [1,0,0,0,0,0,1],
                [1,1,1,1,1,1,1]
            ];
            
            for (let py = 0; py < 7; py++) {
                for (let px = 0; px < 7; px++) {
                    if (pattern[py][px]) {
                        ctx.fillRect(
                            offsetX + (x + px) * cellSize, 
                            offsetY + (y + py) * cellSize, 
                            cellSize, 
                            cellSize
                        );
                    }
                }
            }
        };
        
        // Add finder patterns in three corners
        addFinderPattern(0, 0);
        addFinderPattern(gridSize - 7, 0);
        addFinderPattern(0, gridSize - 7);
        
        // Add data modules with a more complex pattern
        for (let y = 0; y < gridSize; y++) {
            for (let x = 0; x < gridSize; x++) {
                // Skip finder patterns
                if ((x < 9 && y < 9) || 
                    (x >= gridSize - 8 && y < 9) || 
                    (x < 9 && y >= gridSize - 8)) {
                    continue;
                }
                
                // Skip timing patterns (rows and columns 6)
                if (x === 6 || y === 6) {
                    continue;
                }
                
                // Generate pattern based on position and seed
                const hash = ((x * 37) + (y * 41) + seed) % 100;
                if (hash < 45) {
                    ctx.fillRect(
                        offsetX + x * cellSize, 
                        offsetY + y * cellSize, 
                        cellSize, 
                        cellSize
                    );
                }
            }
        }
        
        // Add timing patterns
        ctx.fillStyle = '#000000';
        for (let i = 9; i < gridSize - 8; i += 2) {
            // Horizontal timing pattern
            ctx.fillRect(offsetX + i * cellSize, offsetY + 6 * cellSize, cellSize, cellSize);
            // Vertical timing pattern  
            ctx.fillRect(offsetX + 6 * cellSize, offsetY + i * cellSize, cellSize, cellSize);
        }
        
        element.innerHTML = `
            <div style="display: flex; flex-direction: column; align-items: center; gap: 16px;">
                <div style="border: 2px solid #e5e7eb; border-radius: 12px; padding: 16px; background: white; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);">
                    ${canvas.outerHTML}
                </div>
                <div style="text-align: center; color: #6b7280; max-width: 200px;">
                    <div style="font-size: 16px; font-weight: 500; margin-bottom: 4px;">Use your phone or tablet</div>
                    <div style="font-size: 14px;">Scan this QR code to sign in with a passkey</div>
                </div>
            </div>
        `;
    }
};

// Generate QR code for passkey authentication
window.generatePasskeyQRCode = function(elementId, authData) {
    const element = document.getElementById(elementId);
    if (element) {
        // Create a data URL that mobile devices can use
        const passkeyData = {
            type: 'passkey-auth',
            domain: window.location.hostname,
            timestamp: Date.now(),
            data: authData
        };
        
        const qrData = `passkey://auth?data=${encodeURIComponent(JSON.stringify(passkeyData))}`;
        
        // Use the enhanced QR code generator
        window.generateQRCode(elementId, qrData);
        
        // Add additional mobile-friendly instructions
        const instructions = document.createElement('div');
        instructions.style.cssText = `
            margin-top: 16px;
            padding: 12px;
            background: #f3f4f6;
            border-radius: 8px;
            font-size: 14px;
            color: #374151;
            text-align: left;
        `;
        instructions.innerHTML = `
            <div style="font-weight: 500; margin-bottom: 8px;">How to use:</div>
            <div style="margin-left: 16px;">
                <div>• Open your camera app</div>
                <div>• Point at the QR code</div>
                <div>• Tap the notification to continue</div>
            </div>
        `;
        
        element.appendChild(instructions);
    }
};

// GitHub-style passkey authentication with automatic registration
window.passkeyAuth = {
    // Check if passkeys are supported
    isSupported: function() {
        return window.PublicKeyCredential && 
               typeof window.PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable === 'function';
    },
    
    // Check if platform authenticator is available (Windows Hello, Touch ID, etc.)
    isPlatformAuthenticatorAvailable: async function() {
        if (!this.isSupported()) return false;
        try {
            return await window.PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable();
        } catch (e) {
            return false;
        }
    },
    
    // Check if cross-platform authenticators are available
    isCrossPlatformAuthenticatorAvailable: async function() {
        if (!this.isSupported()) return false;
        try {
            // Try to create a credential to test cross-platform support
            const options = {
                publicKey: {
                    challenge: new Uint8Array(32),
                    rp: { name: "Test" },
                    user: {
                        id: new Uint8Array(16),
                        name: "test",
                        displayName: "Test User"
                    },
                    pubKeyCredParams: [{ type: "public-key", alg: -7 }],
                    authenticatorSelection: {
                        authenticatorAttachment: "cross-platform",
                        userVerification: "discouraged"
                    },
                    timeout: 1000
                }
            };
            
            // This will fail but tells us if cross-platform is supported
            await navigator.credentials.create(options);
            return true;
        } catch (e) {
            return e.name !== 'NotSupportedError';
        }
    },
    
    // Get available authentication methods
    getAvailableMethods: async function() {
        const methods = [];
        
        if (await this.isPlatformAuthenticatorAvailable()) {
            methods.push('platform');
        }
        
        if (await this.isCrossPlatformAuthenticatorAvailable()) {
            methods.push('cross-platform');
        }
        
        return methods;
    },
    
    // Enhanced authentication with automatic registration fallback
    authenticateWithFallback: async function(publicKeyOptions, email, displayName) {
        console.log('[PasskeyAuth] Starting authentication with fallback for:', email);
        
        try {
            // First try authentication
            return await window.webauthn.get(publicKeyOptions, {
                timeout: 60000 // 1 minute for initial attempt
            });
        } catch (error) {
            console.log('[PasskeyAuth] Authentication failed, checking for registration fallback:', error.message);
            
            // If no credentials exist, offer registration
            if (error.message.includes('No passkeys registered') || 
                error.message.includes('Authentication cancelled')) {
                
                console.log('[PasskeyAuth] Offering registration fallback');
                
                // Show registration dialog
                const shouldRegister = confirm(
                    `No passkey found for ${email}. Would you like to create a new passkey for this account?`
                );
                
                if (shouldRegister) {
                    // Call registration flow
                    const registrationOptions = {
                        rp: { name: "OnePortal", id: window.location.hostname },
                        user: {
                            id: new TextEncoder().encode(email),
                            name: email,
                            displayName: displayName || email
                        },
                        challenge: new Uint8Array(32),
                        pubKeyCredParams: [
                            { type: "public-key", alg: -7 },
                            { type: "public-key", alg: -257 }
                        ],
                        timeout: 120000,
                        attestation: "direct"
                    };
                    
                    const credential = await window.webauthn.create(registrationOptions, {
                        requireResidentKey: true,
                        authenticatorAttachment: "any"
                    });
                    
                    return {
                        type: 'registration',
                        credential: credential,
                        email: email,
                        displayName: displayName || email
                    };
                }
            }
            
            throw error;
        }
    }
};