function getContainer(dataGridId) {
    const dataGrid = document.getElementById(dataGridId);
    return dataGrid.getElementsByClassName('rz-data-grid-data')[0];
}

function getContainerMetrics(dataGridId, rowHeight) {
    const container = getContainer(dataGridId);
    const tHead = container.getElementsByTagName('thead')[0];

    // subtract table header from client height
    const clientHeight = container.clientHeight - tHead.offsetHeight;

    const maxRowCount = Math.floor(clientHeight / rowHeight);
    const currentScrollPosition = container.scrollTop;
    const firstScrollRow = Math.ceil(currentScrollPosition / rowHeight);
    const lastScrollRow = (firstScrollRow + maxRowCount) - 1;

    return {
        container: container,
        clientHeight: clientHeight,
        maxRowCount: maxRowCount,
        firstScrollRow: firstScrollRow,
        lastScrollRow: lastScrollRow
    }
}

export function getRowHeight(dataGridId) {
    const container = getContainer(dataGridId);
    const rows = container.getElementsByClassName('rz-data-row');

    if (rows.length === 0) return 0;
    return rows[0].offsetHeight;
}

export function scrollToRow(dataGridId, rowIndex, rowHeight, instant) {
    const container = getContainer(dataGridId);

    const scrollRow = rowIndex * rowHeight;

    container.scrollTo({
        top: scrollRow,
        behavior: instant ? 'instant' : 'smooth' // Optional: for smooth scrolling
    });
}

export function scrollRowIntoView(dataGridId, newScrollRow, rowHeight, instant) {

    const metrics = getContainerMetrics(dataGridId, rowHeight);

    let newTopRow = 0;
    if (newScrollRow < metrics.firstScrollRow) {
        newTopRow = newScrollRow;
    }
    else if (newScrollRow > metrics.lastScrollRow) {
        newTopRow = (newScrollRow - metrics.maxRowCount) + 1;
    }
    else {
        return;
    }

    const scrollPosition = Math.max(0, Math.min(newTopRow * rowHeight, metrics.container.scrollHeight));

    metrics.container.scrollTo({
        top: scrollPosition,
        behavior: instant ? 'instant' : 'smooth' // Optional: for smooth scrolling
    });
}

export function getTopRowIndex(dataGridId, rowHeight) {
    const metrics = getContainerMetrics(dataGridId, rowHeight);

    return metrics.firstScrollRow;
}

export function setTopRowIndex(dataGridId, topRowIndex, rowHeight, instant) {
    const metrics = getContainerMetrics(dataGridId, rowHeight);

    const scrollPosition = Math.max(0, Math.min(topRowIndex * rowHeight, metrics.container.scrollHeight));
    console.log(topRowIndex, rowHeight, scrollPosition);
    metrics.container.scrollTo({
        top: scrollPosition,
        behavior: instant ? 'instant' : 'smooth' // Optional: for smooth scrolling
    });
}

export function getLeftScrollPosition(dataGridId) {
    const container = getContainer(dataGridId);

    return container.scrollLeft;
}

export function setLeftScrollPosition(dataGridId, position, instant) {
    const container = getContainer(dataGridId);

    const scrollPosition = Math.max(0, Math.min(position, container.scrollWidth));

    container.scrollTo({
        left: scrollPosition,
        behavior: instant ? 'instant' : 'smooth' // Optional: for smooth scrolling
    });
}