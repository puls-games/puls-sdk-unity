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

    // Local Storage
    Puls_SaveToStorage: function(key, value) {
        const keyStr = UTF8ToString(key);
        const valueStr = UTF8ToString(value);
        localStorage.setItem(keyStr, valueStr);
    },

    Puls_LoadFromStorage: function(key) {
        const keyStr = UTF8ToString(key);
        const value = localStorage.getItem(keyStr);
        if (value === null) return 0;
        
        const buffer = _malloc(lengthBytesUTF8(value) + 1);
        stringToUTF8(value, buffer, lengthBytesUTF8(value) + 1);
        return buffer;
    },

    Puls_RemoveFromStorage: function(key) {
        const keyStr = UTF8ToString(key);
        localStorage.removeItem(keyStr);
    },

    Puls_ClearStorage: function() {
        localStorage.clear();
    },

    // Puls Storage
    Puls_SaveToPulsStorage: function(key, value) {
        const keyStr = UTF8ToString(key);
        const valueStr = UTF8ToString(value);
        
        if (window.puls && window.puls.storage) {
            window.puls.storage.set(keyStr, valueStr)
                .catch(error => console.error('PulsStorage error:', error));
        }
    },

    Puls_LoadFromPulsStorage: function(key, callback) {
        const keyStr = UTF8ToString(key);
        
        if (!window.puls || !window.puls.storage) {
            return 0;
        }

        window.puls.storage.get(keyStr)
            .then(value => {
                if (value === null || value === undefined) return 0;
                
                const buffer = _malloc(lengthBytesUTF8(value) + 1);
                stringToUTF8(value, buffer, lengthBytesUTF8(value) + 1);
                Runtime.dynCall('i', callback, [buffer]);
            })
            .catch(error => {
                console.error('PulsStorage load error:', error);
                Runtime.dynCall('i', callback, [0]);
            });
        
        return 1; // Возвращаем 1, чтобы показать, что запрос начат
    },

    Puls_FreeMemory: function(buffer) {
        _free(buffer);
    }
});