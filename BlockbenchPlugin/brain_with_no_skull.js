
let codec = null;
let export_action = null;
let format = null;

function setup_codec() {
    codec = new Codec("blockymodel", {
        name: "Brain With No Skull",
        extension: "json",
        remember: true,
        load_filter: {
            type: "json",
            extensions: ["json"]
        },
        compile(options = {}) {
            let model = {
                nodes: []
            };

            function transform_vector(vec) {
                result = [vec[0] + 4, vec[1], vec[2] + 4]
                result[0] /= 8
                result[1] /= 8
                result[2] /= 8
                return result;
            }

            function transform_uv(uv) {
                return [uv[0] / Project.texture_width, uv[1] / Project.texture_height];
            }

            let nodes = Outliner.root.filter((node) => node instanceof Cube);
            for (let node of nodes) {
                let from = transform_vector(node.from)
                let to = transform_vector(node.to)
                let element = {
                    name: node.name,
                    id: node.uuid,
                    quads: []
                };
                let faces = node.faces;
                if ("north" in faces) {
                    let face = faces.north;
                    element.quads.push(
                        {
                            vertices: [
                                {
                                    pos: [from[0], from[1], from[2]],
                                    uv: transform_uv([face.uv[2], face.uv[3]])
                                },
                                {
                                    pos: [to[0], from[1], from[2]],
                                    uv: transform_uv([face.uv[0], face.uv[3]])
                                },
                                {
                                    pos: [to[0], to[1], from[2]],
                                    uv: transform_uv([face.uv[0], face.uv[1]])
                                },
                                {
                                    pos: [from[0], to[1], from[2]],
                                    uv: transform_uv([face.uv[2], face.uv[1]])
                                }
                            ],
                            texture: 0,
                            normal: [0, 0, 1]
                        })
                }
                if ("south" in faces) {
                    let face = faces.south;
                    element.quads.push(
                        {
                            vertices: [
                                {
                                    pos: [from[0], from[1], to[2]],
                                    uv: transform_uv([face.uv[0], face.uv[3]])
                                },
                                {
                                    pos: [from[0], to[1], to[2]],
                                    uv: transform_uv([face.uv[0], face.uv[1]])
                                },
                                {
                                    pos: [to[0], to[1], to[2]],
                                    uv: transform_uv([face.uv[2], face.uv[1]])
                                },
                                {
                                    pos: [to[0], from[1], to[2]],
                                    uv: transform_uv([face.uv[2], face.uv[3]])
                                }
                            ],
                            texture: 0,
                            normal: [0, 0, -1]
                        })
                }
                if ("east" in faces) {
                    let face = faces.east;
                    element.quads.push(
                        {
                            vertices: [
                                {
                                    pos: [to[0], from[1], from[2]],
                                    uv: transform_uv([face.uv[2], face.uv[3]])
                                },
                                {
                                    pos: [to[0], from[1], to[2]],
                                    uv: transform_uv([face.uv[0], face.uv[3]])
                                },
                                {
                                    pos: [to[0], to[1], to[2]],
                                    uv: transform_uv([face.uv[0], face.uv[1]])
                                },
                                {
                                    pos: [to[0], to[1], from[2]],
                                    uv: transform_uv([face.uv[2], face.uv[1]])
                                }

                            ],
                            texture: 0,
                            normal: [-1, 0, 0]
                        })
                }
                if ("west" in faces) {
                    let face = faces.west;
                    element.quads.push(
                        {
                            vertices: [
                                {
                                    pos: [from[0], from[1], from[2]],
                                    uv: transform_uv([face.uv[0], face.uv[3]])
                                },
                                {
                                    pos: [from[0], to[1], from[2]],
                                    uv: transform_uv([face.uv[0], face.uv[1]])
                                },
                                {
                                    pos: [from[0], to[1], to[2]],
                                    uv: transform_uv([face.uv[2], face.uv[1]])
                                },
                                {
                                    pos: [from[0], from[1], to[2]],
                                    uv: transform_uv([face.uv[2], face.uv[3]])
                                }
                            ],
                            texture: 0,
                            normal: [1, 0, 0]
                        })
                }
                if ("up" in faces) {
                    let face = faces.up;
                    element.quads.push(
                        {
                            vertices: [
                                {
                                    pos: [from[0], to[1], from[2]],
                                    uv: transform_uv([face.uv[0], face.uv[1]])
                                },
                                {
                                    pos: [to[0], to[1], from[2]],
                                    uv: transform_uv([face.uv[2], face.uv[1]])
                                },
                                {
                                    pos: [to[0], to[1], to[2]],
                                    uv: transform_uv([face.uv[2], face.uv[3]])
                                },
                                {
                                    pos: [from[0], to[1], to[2]],
                                    uv: transform_uv([face.uv[0], face.uv[3]])
                                }
                            ],
                            texture: 0,
                            normal: [0, -1, 0]
                        })
                }

                if ("down" in faces) {
                    let face = faces.down;
                    element.quads.push(
                        {
                            vertices: [
                                {
                                    pos: [from[0], from[1], from[2]],
                                    uv: transform_uv([face.uv[0], face.uv[3]])
                                },
                                {
                                    pos: [from[0], from[1], to[2]],
                                    uv: transform_uv([face.uv[0], face.uv[1]])
                                },
                                {
                                    pos: [to[0], from[1], to[2]],
                                    uv: transform_uv([face.uv[2], face.uv[1]])
                                },
                                {
                                    pos: [to[0], from[1], from[2]],
                                    uv: transform_uv([face.uv[2], face.uv[3]])
                                }
                            ],
                            texture: 0,
                            normal: [0, 1, 0]
                        })
                }

                model.nodes.push(element);
            }

            if (options.raw) {
                return model;
            } else {
                return compileJSON(model, {
                    indentation: "  ",
                    final_newline: false
                });
            }
        }
    });
    export_action = new Action("export_bwns", {
        name: "Export Brain With No Skull Format",
        description: "",
        icon: "fa-brain",
        category: "file",
        condition: {formats: ["brain_with_no_skull"]},
        click: function () {
            codec.export();
        }
    });
    codec.export_action = export_action;
    MenuBar.menus.file.addAction(export_action, "export.1");
    return codec;
}

function setup_format() {
    format = new ModelFormat("brain_with_no_skull", {
        category: "brain_with_no_skull",
        target: "Brain With No Skull",
        forward_direction: "+x",
        single_texture_default: true,
        animation_files: false,
        bone_rig: false,
        centered_grid: true,
        box_uv: false,
        optional_box_uv: true,
        box_uv_float_size: true,
        integer_size: true,
        uv_rotation: true,
        rotate_cubes: true,
        per_texture_uv_size: true,
        texture_wrap_default: "clamp",
        stretch_cubes: true,
        model_identifier: false,
        name: "Brain With No Skull",
        description: "",
        icon: "fa-brain",
        block_size: 8,
        codec: setup_codec()
    });
}

Plugin.register('brain_with_no_skull', {
    title: 'Brain With No Skull',
    author: 'Tiffit',
    icon: 'fa-brain',
    description: '',
    version: '1.0.0',
    variant: 'both',
    onload() {
        setup_format();
    },
    onunload(){
        codec.delete();
        export_action.delete();
        format.delete();
    }
});