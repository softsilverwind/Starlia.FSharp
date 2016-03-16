module Starlia.Simple.Shaders
    let basiccolor_v = """
        #version 120

        attribute vec3 pos;
        attribute vec3 color;

        uniform mat4 wvp;
        varying vec3 f_color;

        void main(void)
        {
            gl_Position = wvp * vec4(pos, 1.0);
            f_color = color;
        }
    """

    let basiccolor_f = """
        #version 120

        varying vec3 f_color;

        void main(void)
        {
            gl_FragColor = vec4(f_color, 1.0);
        }
    """

    let basicobject_v = """
        #version 120
        attribute vec3 pos;
        attribute vec2 texcoord;

        varying vec2 f_texcoord;

        uniform mat4 wvp;

        void main(void)
        {
            gl_Position = wvp * vec4(pos, 1.0);
            f_texcoord = texcoord;
        }
    """

    let basicobject_f = """
        #version 120

        varying vec2 f_texcoord;
        uniform sampler2D tex;

        void main(void)
        {
            vec2 flipped_texcoord = vec2(f_texcoord.x, f_texcoord.y);
            gl_FragColor = texture2D(tex, flipped_texcoord);
        }
    """
