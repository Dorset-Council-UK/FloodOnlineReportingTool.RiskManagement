import "./app.scss";

type StreamReference = {
    arrayBuffer: () => Promise<ArrayBuffer>;
};

declare global {
    interface Window {
        downloadFileFromStream: (fileName: string, contentStreamReference: StreamReference) => Promise<void>;
    }
}

// Function based on https://learn.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-10.0
window.downloadFileFromStream = async (fileName: string, contentStreamReference: StreamReference): Promise<void> =>
{
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    const anchorElement = document.createElement("a");
    anchorElement.href = url;
    anchorElement.download = fileName ?? "";
    anchorElement.click();
    anchorElement.remove();

    URL.revokeObjectURL(url);
};

export {};
