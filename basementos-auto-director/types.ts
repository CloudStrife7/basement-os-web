export interface DirectorConfig {
  characterIdentity: string;
  characterVisuals: string;
  artStyle: string;
  script: string;
}

export interface Scene {
  sceneNumber: string;
  title: string;
  imageCommand: string;
  videoCommand: string;
}

export interface ManifestResponse {
  scenes: Scene[];
}
