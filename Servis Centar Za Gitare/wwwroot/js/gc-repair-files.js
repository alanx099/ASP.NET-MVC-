(function () {
    const containers = document.querySelectorAll("[data-repair-files]");

    containers.forEach((container) => {
        const listUrl = container.dataset.listUrl;
        const uploadUrl = container.dataset.uploadUrl;
        const list = container.querySelector("[data-repair-file-list]");
        const error = container.querySelector("[data-repair-files-error]");
        const tokenInput = container.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenInput ? tokenInput.value : "";
        const dropzoneForm = container.querySelector("[data-repair-dropzone]");
        const uploadButton = container.querySelector("[data-repair-upload-button]");
        const progress = container.querySelector("[data-repair-upload-progress]");
        const progressBar = container.querySelector("[data-repair-upload-progress-bar]");
        const repairForm = document.querySelector("[data-repair-form]");
        let dropzone = null;
        let uploadInProgress = false;
        let submitAfterUpload = false;
        let allowRepairSubmit = false;
        let hadUploadError = false;
        let uploadStartedAt = 0;
        let finalizingUpload = false;

        const showError = (message) => {
            if (error) {
                const text = message || "";
                error.textContent = text;
                error.hidden = !text;
                error.classList.toggle("field-validation-error", Boolean(text));
                error.classList.toggle("field-validation-valid", !text);
            }
        };

        const formatSize = (size) => {
            if (size < 1024) {
                return `${size} B`;
            }

            if (size < 1024 * 1024) {
                return `${(size / 1024).toFixed(1)} KB`;
            }

            return `${(size / 1024 / 1024).toFixed(1)} MB`;
        };

        const setProgress = (value) => {
            if (!progress || !progressBar) {
                return;
            }

            const safeValue = Math.max(0, Math.min(100, value));
            progress.hidden = safeValue <= 0 || safeValue >= 100;
            progressBar.style.width = `${safeValue}%`;
        };

        const pendingFiles = () => {
            if (!dropzone || !window.Dropzone) {
                return [];
            }

            return dropzone.files.filter((file) =>
                file.status === window.Dropzone.ADDED ||
                file.status === window.Dropzone.QUEUED);
        };

        const updateUploadButton = () => {
            if (!uploadButton) {
                return;
            }

            uploadButton.disabled = uploadInProgress || pendingFiles().length === 0;
        };

        const processNextBatch = () => {
            if (!dropzone || hadUploadError || pendingFiles().length === 0) {
                return;
            }

            dropzone.processQueue();
        };

        const finishUpload = async () => {
            if (!dropzone || finalizingUpload) {
                return;
            }

            finalizingUpload = true;
            uploadInProgress = false;
            updateUploadButton();

            const minimumVisibleTime = 900;
            const elapsed = Date.now() - uploadStartedAt;
            const remaining = Math.max(0, minimumVisibleTime - elapsed);

            setProgress(96);

            await new Promise((resolve) => setTimeout(resolve, remaining));
            setProgress(100);
            await new Promise((resolve) => setTimeout(resolve, 320));

            if (!hadUploadError) {
                dropzone.removeAllFiles(true);
                await loadFiles();

                if (submitAfterUpload && repairForm) {
                    allowRepairSubmit = true;
                    repairForm.requestSubmit();
                }
            }

            submitAfterUpload = false;
            finalizingUpload = false;
            setTimeout(() => setProgress(0), 220);
        };

        const startUpload = (afterUpload) => {
            if (!dropzone || uploadInProgress) {
                return false;
            }

            if (pendingFiles().length === 0) {
                showError("Add images to the drop zone before uploading.");
                return false;
            }

            submitAfterUpload = Boolean(afterUpload);
            uploadInProgress = true;
            hadUploadError = false;
            finalizingUpload = false;
            uploadStartedAt = Date.now();
            showError("");
            setProgress(1);
            updateUploadButton();
            processNextBatch();
            return true;
        };

        const renderFiles = (files) => {
            if (!list) {
                return;
            }

            list.innerHTML = "";

            if (!files.length) {
                const empty = document.createElement("p");
                empty.className = "section-copy gc-file-empty";
                empty.textContent = "No images uploaded.";
                list.appendChild(empty);
                return;
            }

            files.forEach((file) => {
                const item = document.createElement("article");
                item.className = "gc-file-card";

                const link = document.createElement("a");
                link.href = file.url;
                link.target = "_blank";
                link.rel = "noreferrer";
                link.className = "gc-file-card__thumb";

                const image = document.createElement("img");
                image.src = file.url;
                image.alt = file.name;
                image.loading = "lazy";
                link.appendChild(image);

                const meta = document.createElement("div");
                meta.className = "gc-file-card__meta";

                const name = document.createElement("p");
                name.className = "gc-file-card__name";
                name.textContent = file.name;

                const size = document.createElement("p");
                size.className = "section-copy";
                size.textContent = formatSize(file.size);

                const remove = document.createElement("button");
                remove.type = "button";
                remove.className = "btn gc-btn-secondary gc-file-card__delete";
                remove.textContent = "Delete";
                remove.addEventListener("click", async () => {
                    showError("");
                    remove.disabled = true;

                    const response = await fetch(file.deleteUrl, {
                        method: "DELETE",
                        headers: {
                            "RequestVerificationToken": token
                        }
                    });

                    if (!response.ok) {
                        remove.disabled = false;
                        showError("Image could not be deleted.");
                        return;
                    }

                    await loadFiles();
                });

                meta.appendChild(name);
                meta.appendChild(size);
                meta.appendChild(remove);
                item.appendChild(link);
                item.appendChild(meta);
                list.appendChild(item);
            });
        };

        const loadFiles = async () => {
            showError("");

            const response = await fetch(listUrl, {
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                showError("Images could not be loaded.");
                return;
            }

            renderFiles(await response.json());
        };

        const createFallbackInput = () => {
            if (!dropzoneForm) {
                return;
            }

            const input = document.createElement("input");
            input.type = "file";
            input.name = "file";
            input.accept = "image/jpeg,image/png,image/gif,image/webp";
            input.multiple = true;
            input.className = "form-control gc-input";
            dropzoneForm.appendChild(input);

            input.addEventListener("change", async () => {
                for (const file of input.files) {
                    const formData = new FormData();
                    formData.append("file", file);

                    const response = await fetch(uploadUrl, {
                        method: "POST",
                        headers: {
                            "RequestVerificationToken": token
                        },
                        body: formData
                    });

                    if (!response.ok) {
                        showError("Image could not be uploaded.");
                        break;
                    }
                }

                input.value = "";
                await loadFiles();
            });
        };

        if (window.Dropzone && dropzoneForm) {
            window.Dropzone.autoDiscover = false;

            dropzone = new window.Dropzone(dropzoneForm, {
                url: uploadUrl,
                paramName: "file",
                acceptedFiles: "image/jpeg,image/png,image/gif,image/webp",
                maxFilesize: 10,
                parallelUploads: 1,
                uploadMultiple: false,
                autoProcessQueue: false,
                addRemoveLinks: true,
                dictRemoveFile: "Delete",
                headers: {
                    "RequestVerificationToken": token
                },
                init: function () {
                    this.on("addedfile", () => {
                        showError("");
                        updateUploadButton();
                    });

                    this.on("removedfile", () => {
                        updateUploadButton();
                    });

                    this.on("totaluploadprogress", (progressValue) => {
                        if (uploadInProgress) {
                            setProgress(progressValue);
                        }
                    });

                    this.on("error", (file, response) => {
                        hadUploadError = true;
                        uploadInProgress = false;
                        const message = response && response.message ? response.message : "Image could not be uploaded.";
                        showError(message);
                        updateUploadButton();
                    });

                    this.on("complete", () => {
                        if (!uploadInProgress || hadUploadError) {
                            return;
                        }

                        if (pendingFiles().length > 0) {
                            setTimeout(processNextBatch, 80);
                            return;
                        }

                        if (this.getUploadingFiles().length === 0) {
                            finishUpload();
                        }
                    });

                    this.on("queuecomplete", () => {
                        if (uploadInProgress && !hadUploadError && pendingFiles().length === 0 && this.getUploadingFiles().length === 0) {
                            finishUpload();
                        }
                    });
                }
            });

            if (uploadButton) {
                uploadButton.addEventListener("click", () => {
                    startUpload(false);
                });
            }

            if (repairForm) {
                repairForm.addEventListener("submit", (event) => {
                    if (allowRepairSubmit) {
                        allowRepairSubmit = false;
                        return;
                    }

                    if (startUpload(true)) {
                        event.preventDefault();
                    }
                });
            }
        } else {
            createFallbackInput();
        }

        loadFiles();
    });
})();
