let canvas;
let ctx;
const cellSize = 50;
export function Init() {
    canvas = document.getElementById("canvas-fill");
    ctx = canvas.getContext("2d");
    DrawGrid();
}
export function FillCell(x, y, color) {
    const cellX = x * cellSize;
    const cellY = y * cellSize;
    ctx.fillStyle = color;
    ctx.fillRect(cellX, cellY, cellSize, cellSize);
}
function DrawGrid() {
    const numRows = Math.floor(canvas.height / cellSize);
    const numCols = Math.floor(canvas.width / cellSize);
    for (let i = 0; i <= numRows; i++) {
        ctx.beginPath();
        ctx.moveTo(0, i * cellSize);
        ctx.lineTo(canvas.width, i * cellSize);
        ctx.stroke();
    }
    for (let j = 0; j <= numCols; j++) {
        ctx.beginPath();
        ctx.moveTo(j * cellSize, 0);
        ctx.lineTo(j * cellSize, canvas.height);
        ctx.stroke();
    }
    for (let i = 0; i < numRows; i++) {
        for (let j = 0; j < numCols; j++) {
            const cellX = j * cellSize;
            const cellY = i * cellSize;
            ctx.fillText(`(${i},${j})`, cellX + 5, cellY + cellSize - 5);
        }
    }
}
//# sourceMappingURL=CanvasFill.js.map