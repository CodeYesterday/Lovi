function getContainer(dataGridId) {
    const dataGrid = document.getElementById(dataGridId);
    return dataGrid.getElementsByClassName('rz-data-grid-data')[0];
}

export function getRowHeight(dataGridId) {
    const container = getContainer(dataGridId);
    const rows = container.getElementsByClassName('rz-data-row');

    if (rows.length === 0) return 0;
    return rows[0].offsetHeight;
}

export function scrollToRow(dataGridId, rowIndex, rowHeight) {
    const container = getContainer(dataGridId);

    const scrollRow = rowIndex * rowHeight;

    container.scrollTo({
        top: scrollRow,
        behavior: 'smooth' // Optional: for smooth scrolling
    });
}

export function scrollRowIntoView(dataGridId, newScrollRow, rowHeight) {
    const container = getContainer(dataGridId);
    const tHead = container.getElementsByTagName('thead')[0];

    // subtract table header from client height
    const clientHeight = container.clientHeight - tHead.offsetHeight;
    
    const maxRowCount = Math.floor(clientHeight / rowHeight);
    const currentScrollPosition = container.scrollTop;
    const firstScrollRow = Math.ceil(currentScrollPosition / rowHeight);
    const lastScrollRow = (firstScrollRow + maxRowCount) - 1;

    let newTopRow = 0;
    if (newScrollRow < firstScrollRow) {
        newTopRow = newScrollRow;
    }
    else if (newScrollRow > lastScrollRow) {
        newTopRow = (newScrollRow - maxRowCount) + 1;
    }
    else {
        return;
    }

    const scrollPosition = Math.max(0, Math.min(newTopRow * rowHeight, container.scrollHeight));

    container.scrollTo({
        top: scrollPosition,
        behavior: 'smooth' // Optional: for smooth scrolling
    });
}
