export function addKeyDownListener(objRef) {
    document.addEventListener('keydown', (e) => {
        // prevent some key default actions comming from the MAUI web view
        if (e.code === 'F5'
            || e.ctrlKey && !e.shiftKey && !e.altKey && e.code === 'KeyF'
            || e.ctrlKey && !e.altKey && e.code === 'KeyG'
        ) {
            e.preventDefault();
        }

        objRef.invokeMethodAsync('OnKeyEvent', e.code, e.shiftKey, e.altKey, e.ctrlKey);
    });
}