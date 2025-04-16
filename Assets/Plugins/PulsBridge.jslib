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
     Puls_SaveToCloud: function(dataPtr, successCallback, errorCallback) {
        try {
            if (!window.puls || !window.puls.storage) {
                throw new Error("PULS_STORAGE_NOT_AVAILABLE");
            }

            var data = UTF8ToString(dataPtr);
            
            window.puls.storage.save(data)
                .then(function(syncToken) {
                    var buffer = _malloc(lengthBytesUTF8(syncToken) + 1);
                    stringToUTF8(syncToken, buffer, lengthBytesUTF8(syncToken) + 1);
                    dynCall('vi', successCallback, [buffer]);
                    _free(buffer);
                })
                .catch(function(error) {
                    var errorMsg = error.message || 'UNKNOWN';
                    var buffer = _malloc(lengthBytesUTF8(errorMsg) + 1);
                    stringToUTF8(errorMsg, buffer, lengthBytesUTF8(errorMsg) + 1);
                    dynCall('vi', errorCallback, [buffer]);
                    _free(buffer);
                });
        } catch (e) {
            var errorMsg = e.message || 'UNKNOWN';
            var buffer = _malloc(lengthBytesUTF8(errorMsg) + 1);
            stringToUTF8(errorMsg, buffer, lengthBytesUTF8(errorMsg) + 1);
            dynCall('vi', errorCallback, [buffer]);
            _free(buffer);
        }
    },

    // Puls Cloud Storage - Load
    Puls_LoadFromCloud: function(syncTokenPtr, successCallback, errorCallback) {
        try {
            if (!window.puls || !window.puls.storage) {
                throw new Error("PULS_STORAGE_NOT_AVAILABLE");
            }

            var syncToken = syncTokenPtr ? UTF8ToString(syncTokenPtr) : null;
            
            window.puls.storage.load(syncToken || undefined)
                .then(function(response) {
                    var data = response.data || '';
                    var newSyncToken = response.sync || '';
                    
                    var dataBuffer = _malloc(lengthBytesUTF8(data) + 1);
                    stringToUTF8(data, dataBuffer, lengthBytesUTF8(data) + 1);
                    
                    var tokenBuffer = _malloc(lengthBytesUTF8(newSyncToken) + 1);
                    stringToUTF8(newSyncToken, tokenBuffer, lengthBytesUTF8(newSyncToken) + 1);
                    
                    dynCall('vii', successCallback, [dataBuffer, tokenBuffer]);
                    
                    _free(dataBuffer);
                    _free(tokenBuffer);
                })
                .catch(function(error) {
                    var errorMsg = error.message || 'UNKNOWN';
                    var buffer = _malloc(lengthBytesUTF8(errorMsg) + 1);
                    stringToUTF8(errorMsg, buffer, lengthBytesUTF8(errorMsg) + 1);
                    dynCall('vi', errorCallback, [buffer]);
                    _free(buffer);
                });
        } catch (e) {
            var errorMsg = e.message || 'UNKNOWN';
            var buffer = _malloc(lengthBytesUTF8(errorMsg) + 1);
            stringToUTF8(errorMsg, buffer, lengthBytesUTF8(errorMsg) + 1);
            dynCall('vi', errorCallback, [buffer]);
            _free(buffer);
        }
    },


    Puls_FreeMemory: function(buffer) {
        _free(buffer);
    }
});