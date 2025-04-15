mergeInto(LibraryManager.library, {
    // Platform detection
    Puls_IsMobile: function() {
        return window.puls && window.puls.platform && window.puls.platform.isMobile ? 1 : 0;
    },

    Puls_IsDesktop: function() {
        return window.puls && window.puls.platform && window.puls.platform.isDesktop ? 1 : 0;
    },

    Puls_GetLanguage: function() {
        var language = (window.puls && window.puls.platform && window.puls.platform.language) || 'en-US';
        var buffer = _malloc(lengthBytesUTF8(language) + 1);
        stringToUTF8(language, buffer, lengthBytesUTF8(language) + 1);
        return buffer;
    },

    Puls_GetUserId: function() {
        var userId = (window.puls && window.puls.user && window.puls.user.id) || 'anonymous';
        var buffer = _malloc(lengthBytesUTF8(userId) + 1);
        stringToUTF8(userId, buffer, lengthBytesUTF8(userId) + 1);
        return buffer;
    },

    // Local Storage
    Puls_SaveToLocalStorage: function(userIdPtr, keyPtr, valuePtr) {
        const userId = UTF8ToString(userIdPtr);
        const key = UTF8ToString(keyPtr);
        const value = UTF8ToString(valuePtr);
        localStorage.setItem(`puls_${userId}_${key}`, value);
    },

    Puls_LoadFromLocalStorage: function(userIdPtr, keyPtr) {
        const userId = UTF8ToString(userIdPtr);
        const key = UTF8ToString(keyPtr);
        const value = localStorage.getItem(`puls_${userId}_${key}`);
        if (value === null) return 0;
        
        const buffer = _malloc(lengthBytesUTF8(value) + 1);
        stringToUTF8(value, buffer, lengthBytesUTF8(value) + 1);
        return buffer;
    },

    Puls_RemoveFromLocalStorage: function(userIdPtr, keyPtr) {
        const userId = UTF8ToString(userIdPtr);
        const key = UTF8ToString(keyPtr);
        localStorage.removeItem(`puls_${userId}_${key}`);
    },

    Puls_ClearUserLocalStorage: function(userIdPtr) {
        const userId = UTF8ToString(userIdPtr);
        const prefix = `puls_${userId}_`;
        Object.keys(localStorage).forEach(key => {
            if (key.startsWith(prefix)) {
                localStorage.removeItem(key);
            }
        });
    },

    // Puls Cloud Storage
    Puls_SaveToCloud: function(userIdPtr, dataPtr, callback, errorCallback) {
        const userId = UTF8ToString(userIdPtr);
        const data = UTF8ToString(dataPtr);
        
        if (!window.puls || !window.puls.storage) {
            Runtime.dynCall('v', errorCallback, ["UNKNOWN"]);
            return;
        }

        window.puls.storage.save(data)
            .then(syncToken => {
                const buffer = _malloc(lengthBytesUTF8(syncToken) + 1);
                stringToUTF8(syncToken, buffer, lengthBytesUTF8(syncToken) + 1);
                Runtime.dynCall('vi', callback, [buffer]);
            })
            .catch(error => {
                const errorMsg = error.message || 'UNKNOWN';
                const buffer = _malloc(lengthBytesUTF8(errorMsg) + 1);
                stringToUTF8(errorMsg, buffer, lengthBytesUTF8(errorMsg) + 1);
                Runtime.dynCall('vi', errorCallback, [buffer]);
            });
    },

    Puls_LoadFromCloud: function(userIdPtr, syncTokenPtr, callback, errorCallback) {
        const syncToken = UTF8ToString(syncTokenPtr);
        
        if (!window.puls || !window.puls.storage) {
            Runtime.dynCall('v', errorCallback, ["UNKNOWN"]);
            return;
        }

        window.puls.storage.load(syncToken || undefined)
            .then(response => {
                const data = response.data || '';
                const syncToken = response.sync || '';
                
                const dataBuffer = _malloc(lengthBytesUTF8(data) + 1);
                stringToUTF8(data, dataBuffer, lengthBytesUTF8(data) + 1);
                
                const syncBuffer = _malloc(lengthBytesUTF8(syncToken) + 1);
                stringToUTF8(syncToken, syncBuffer, lengthBytesUTF8(syncToken) + 1);
                
                Runtime.dynCall('vii', callback, [dataBuffer, syncBuffer]);
            })
            .catch(error => {
                const errorMsg = error.message || 'UNKNOWN';
                const buffer = _malloc(lengthBytesUTF8(errorMsg) + 1);
                stringToUTF8(errorMsg, buffer, lengthBytesUTF8(errorMsg) + 1);
                Runtime.dynCall('vi', errorCallback, [buffer]);
            });
    },

    Puls_FreeMemory: function(buffer) {
        _free(buffer);
    }
});